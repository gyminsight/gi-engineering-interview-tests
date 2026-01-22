namespace Test1.Exceptions
{
    public class PrimaryMemberException :Exception
    {
        public PrimaryMemberException()
        {
        }
        public PrimaryMemberException(string message)
            : base(message)
        {
        }
        public PrimaryMemberException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
