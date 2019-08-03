using System;
using EventStore.ClientAPI;

namespace Infrastructure
{
    public abstract class CheckPoint : IComparable
    {
        public static bool operator <(CheckPoint cp1, CheckPoint cp2) { return cp1?.CompareTo(cp2) < 0; }
        public static bool operator >(CheckPoint cp1, CheckPoint cp2) { return cp1?.CompareTo(cp2) > 0; }
        public static bool operator ==(CheckPoint cp1, CheckPoint cp2) { return cp1?.CompareTo(cp2) == 0; }
        public static bool operator !=(CheckPoint cp1, CheckPoint cp2) { return cp1?.CompareTo(cp2) != 0; }
        public override bool Equals(object obj)
        {
            if (!(obj is CheckPoint)) return false;
            return this == (CheckPoint)obj;
        }
        public static bool operator <=(CheckPoint cp1, CheckPoint cp2) { return cp1?.CompareTo(cp2) <= 0; }
        public static bool operator >=(CheckPoint cp1, CheckPoint cp2) { return cp1?.CompareTo(cp2) >= 0; }
        public abstract int CompareTo(object obj);
        public abstract override int GetHashCode();
    }

    public class StreamCheckpoint : CheckPoint
    {
        private readonly long _position;

        public StreamCheckpoint(long position)
        {
            _position = position;
        }

        public override int CompareTo(object obj)
        {
            switch (obj) {
                case null:
                    return 1;
                case StreamCheckpoint other:
                    return _position.CompareTo(other._position);
                default:
                    throw new ArgumentException($"Object is not a {nameof(StreamCheckpoint)}");
            }
        }

        public override int GetHashCode()
        {
            return _position.GetHashCode();
        }
    }
    //TODO: fold this into position
    public class AllCheckpoint : CheckPoint
    {
        private readonly long _prepare;
        private readonly long _commit;


        public AllCheckpoint(long prepare, long commit)
        {
            _prepare = prepare;
            _commit = commit;
        }

        public AllCheckpoint(Position position)
        {
            _prepare = position.PreparePosition;
            _commit = position.CommitPosition;
        }

        public override int CompareTo(object obj)
        {
            switch (obj) {
                case null:
                    return 1;
                case AllCheckpoint other:
                    var comparison = _commit.CompareTo(other._commit);
                    if (comparison == 0) {
                        comparison = _prepare.CompareTo(other._prepare);
                    }
                    return comparison;
                default:
                    throw new ArgumentException($"Object is not a {nameof(AllCheckpoint)}");
            }
        }
        public override int GetHashCode()
        {
            return Tuple.Create(_prepare, _commit).GetHashCode();
        }
    }
}