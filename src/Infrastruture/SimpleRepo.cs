using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Infrastructure
{
    public class SimpleRepo : IRepository
    {
        private readonly IEventStoreConnection _conn;
        private readonly string _eventNamespace;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.Auto
        };

        public SimpleRepo(IEventStoreConnection conn, string eventNamespace)
        {
            _conn = conn;
            _eventNamespace = eventNamespace;
        }

        public void Save(IEventSource source)
        {
            var eventData = new List<EventData>();
            foreach (var @event in source.TakeEvents()) {
                eventData.Add(Serialize(@event));
            }
            var stream = $"{source.Name}-{source.Id:N}";
            _conn.AppendToStreamAsync(stream, source.Version, eventData).Wait();
        }

        public T Load<T>(Guid id) where T : class, IEventSource
        {
            T writer = (T)FormatterServices.GetUninitializedObject(typeof(T));
            var streamName = $"{writer.Name}-{id:N}";

            long sliceStart = 0;
            StreamEventsSlice currentSlice;
            do {
                currentSlice = _conn.ReadStreamEventsForwardAsync(
                                        streamName,
                                        sliceStart,
                                        500,
                                        true).Result;

                if (currentSlice.Status == SliceReadStatus.StreamNotFound)
                    throw new Exception($"Stream not found {streamName}");
                if (currentSlice.Status == SliceReadStatus.StreamNotFound)
                    throw new Exception($"Stream has been deleted {streamName}");

                sliceStart = currentSlice.NextEventNumber;
                writer.Hydrate(currentSlice.Events.Select(evt => (IEvent)Deserialize(evt)));

            } while (!currentSlice.IsEndOfStream);

            return writer;

        }
        private EventData Serialize(IEvent @event)
        {
            var dString = JsonConvert.SerializeObject(@event, _serializerSettings);
            var data = Encoding.UTF8.GetBytes(dString);
            var typeName = @event.GetType().Name;
            return new EventData(Guid.NewGuid(), typeName, true, data, null);
        }
        public object Deserialize(ResolvedEvent @event)
        {
            try {
                
                var type = Assembly.GetEntryAssembly()?.GetType($"{_eventNamespace}.{@event.Event.EventType}");
                return JsonConvert.DeserializeObject(
                    Encoding.UTF8.GetString(@event.Event.Data),
                    type,
                    _serializerSettings);
            }
            catch (Exception _) {
                return null;
            }
        }
    }
}
