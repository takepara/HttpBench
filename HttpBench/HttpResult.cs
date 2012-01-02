namespace HttpBench
{
    public class HttpResult
    {
        public int ManagedThreadId { get; set; }
        public int Status { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public int TransferLength { get; set; }
        public string ContentType { get; set; }
    }
}
