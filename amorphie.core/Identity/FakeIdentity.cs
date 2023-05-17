namespace amorphie.core.Identity
{
    public class FakeIdentity : IBBTIdentity
    {
        public Guid? UserId => Guid.NewGuid();

        public Guid? BehalfOfId => Guid.NewGuid();
    }
}
