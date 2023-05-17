namespace amorphie.core.Identity
{
    public interface IBBTIdentity
    {
        public abstract Guid? UserId { get; }

        public abstract Guid? BehalfOfId { get; }
    }
}
