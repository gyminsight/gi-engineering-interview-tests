namespace Test1.Exceptions
{
    public class LastAccountMemberException :Exception
    {
        public LastAccountMemberException()
        {
        }
        public LastAccountMemberException(string message)
            : base(message)
        {
        }
        public LastAccountMemberException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
