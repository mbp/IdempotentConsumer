namespace IdempotentConsumer.Core
{
	using System.Collections.Generic;
	using System.Linq;
	using NServiceBus;

	public class MessageRedispatcher : IRedispatchMessages
	{
		private readonly IBus bus;
		private readonly HashSet<int> alreadyDispatched = new HashSet<int>();

		public MessageRedispatcher(IBus bus)
		{
			this.bus = bus;
		}

		public void Redispatch(IEnumerable<DispatchedMessage> messages)
		{
			foreach (var groupedByDispatchMethod in messages.GroupBy(x => x.GroupIndex))
			{
				var method = groupedByDispatchMethod.First().Method;
				this.bus.Dispatch(method, this.GetUndispatchedMessages(groupedByDispatchMethod));
			}
		}
		private IEnumerable<IMessage> GetUndispatchedMessages(IEnumerable<DispatchedMessage> messages)
		{
			return messages.Where(x => this.MessageNeverDispatched(x)).Select(x => x.Body);
		}
		private bool MessageNeverDispatched(DispatchedMessage message)
		{
			return this.alreadyDispatched.Add(message.MessageIndex);
		}
	}
}