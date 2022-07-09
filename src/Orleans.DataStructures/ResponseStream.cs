using System;

namespace Orleans.DataStructures
{
    public struct ResponseStream
    {
        public Guid ResponseStreamId { get; private set; }

        public ResponseStream(Guid responseStreamId)
        {
            ResponseStreamId = responseStreamId;
        }
    }
}