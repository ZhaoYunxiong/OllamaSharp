using System.Text;

namespace OllamaSharp.Models.Chat;

/// <summary>
/// A builder class for constructing a <see cref="Message"/> by appending multiple message chunks.
/// </summary>
public class MessageBuilder
{
	private readonly StringBuilder _contentBuilder = new();
	private readonly StringBuilder _thinkContentBuilder = new();
	private List<string> _images = [];
	private List<Message.MessageToolCall> _toolCalls = [];

	/// <summary>
	/// Appends a chat response stream chunk to the message under construction.
	/// </summary>
	/// <param name="chunk">The <see cref="ChatResponseStream"/> instance containing a message and additional data to append. If the message is <c>null</c>n, no operation is performed.</param>
	/// <remarks>
	/// This method processes the provided chunk by appending its message content to the underlying content builder,
	/// updates the <see cref="Role"/> based on the chunk's message role, and adds any related images or tool calls if present.
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// var builder = new MessageBuilder();
	/// var chunk = new ChatResponseStream
	/// {
	///		Message = new Message
	///		{
	///			Content = "Hello, World!",
	///			Role = ChatRole.User,
	///			Images = new[] { "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAIAAABLbSncAAAAGUlEQVR4nGJpfnqXARtgwio6aCUAAQAA///KcwJYgRBQbAAAAABJRU5ErkJggg==" },
	///			ToolCalls = new List&lt;Message.ToolCall>()
	///		}
	/// };
	/// builder.Append(chunk);
	/// var resultMessage = builder.ToMessage();
	/// Console.WriteLine(resultMessage.Content); // Output: Hello, World!
	/// </code>
	/// </example>
	public void Append(ChatResponseStream? chunk)
	{
		if (chunk?.Message is null)
			return;

		_contentBuilder.Append(chunk.Message.Content ?? "");
		_thinkContentBuilder.Append(chunk.Message.Thinking ?? "");
		Role = chunk.Message.Role;

		_images.AddRangeIfNotNull(chunk.Message.Images);
		_toolCalls.AddRangeIfNotNull(chunk.Message.ToolCalls);
	}

	/// <summary>
	/// Converts the current state of the message builder into a <see cref="Message"/> object.
	/// </summary>
	/// <returns>
	/// A <see cref="Message"/> instance containing the following elements:
	/// <ul>
	/// <li><see cref="Message.Content"/>: The combined content built using appended chunks.</li>
	/// <li><see cref="Message.Role"/>: The role assigned to the message.</li>
	/// <li><see cref="Message.Images"/>: An array of image strings associated with the message.</li>
	/// <li><see cref="Message.ToolCalls"/>: A collection of tool call objects associated with the message.</li>
	/// </ul>
	/// </returns>
	/// <example>
	/// Example usage:
	/// <code>
	/// var builder = new MessageBuilder();
	/// builder.Role = ChatRole.Assistant;
	/// // Append content (this would typically be done with Append method)
	/// builder.Append(new ChatResponseStream
	/// {
	///	  Message = new Message
	///	  {
	///	    Content = "Generated content from assistant."
	///	  }
	/// });
	/// // Convert to a Message object
	/// var finalMessage = builder.ToMessage();
	/// Console.WriteLine(finalMessage.Content); // Output: Generated content from assistant.
	/// Console.WriteLine(finalMessage.Role);    // Output: Assistant
	/// </code>
	/// </example>
	public Message ToMessage() => new() { Content = _contentBuilder.ToString(), Thinking = _thinkContentBuilder.ToString(), Images = _images.ToArray(), Role = Role, ToolCalls = _toolCalls };


	/// <summary>
	/// Represents the role associated with a chat message. Roles are used to determine the purpose or origin of a message, such as system, user, assistant, or tool.
	/// </summary>
	/// <remarks>
	/// <p>The <see cref="Role"/> property is typically used to indicate the sender's context or role within a conversation.</p>
	/// <lu>
	/// <li><b>System:</b> Represents system-generated messages.</li>
	/// <li><b>User:</b> Represents messages sent by the user.</li>
	/// <li><b>Assistant:</b> Represents messages generated by an assistant or AI model.</li>
	/// <li><b>Tool:</b> Represents messages or actions triggered by external tools.</li>
	/// </lu>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// var messageBuilder = new MessageBuilder();
	/// var chunk = new ChatResponseStream
	/// {
	///	  Message = new Message
	///	  {
	///	    Content = "What can I help you with?",
	///	    Role = ChatRole.Assistant
	///	  }
	/// };
	/// messageBuilder.Append(chunk);
	/// Console.WriteLine(messageBuilder.Role); // Output: Assistant
	/// </code>
	/// </example>
	public ChatRole? Role { get; private set; }

	/// <summary>
	/// Represents the collection of image references included in a message.
	/// </summary>
	/// <remarks>
	/// <p>This property contains a read-only collection of image file paths or URIs associated with the message content.
	/// The <see cref="Images"/> property is often utilized in scenarios where messages include supplementary visual content,
	/// such as in chat interfaces, AI-generated responses with images, or tools requiring multimedia integration.</p>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// var builder = new MessageBuilder();
	/// var chunk = new ChatResponseStream
	/// {
	///	  Message = new Message
	///	  {
	///	    Content = "Here is an example image:",
	///	    Role = ChatRole.Assistant,
	///	    Images = new[] { "example_image.png" } // Note: Images are base64 encoded, this is just an example
	///	  }
	/// };
	/// builder.Append(chunk);
	/// var resultMessage = builder.ToMessage();
	/// Console.WriteLine(string.Join(", ", resultMessage.Images)); // Output: example_image.png
	/// </code>
	/// </example>
	public IReadOnlyCollection<string>? Images { get; }

	/// <summary>
	/// Represents the collection of tool calls associated with a chat message.
	/// </summary>
	/// <remarks>
	/// <p>The <see cref="ToolCalls"/> property is used to store references to external tools or functions invoked during a chat conversation.</p>
	/// Tool calls can include various functions or actions that were triggered by the message. For instance:
	/// <lu>
	/// <li>Fetching data asynchronously from APIs.</li>
	/// <li>Executing background processes.</li>
	/// <li>Triggering integrations with third-party tools.</li>
	/// </lu>
	/// This property aggregates and holds all tool calls appended via the builder during message construction.
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// var messageBuilder = new MessageBuilder();
	/// var toolCall = new Message.ToolCall
	/// {
	///	  Function = new Message.Function()
	/// };
	/// var chunk = new ChatResponseStream
	/// {
	///	  Message = new Message
	///	  {
	///     Content = "Triggered a tool call",
	///     ToolCalls = new[] { toolCall }
	///	  }
	/// };
	/// messageBuilder.Append(chunk);
	/// Console.WriteLine(messageBuilder.ToolCalls.Count); // Output: 1
	/// </code>
	/// </example>
	public IReadOnlyCollection<Message.MessageToolCall>? ToolCalls { get; }

	/// <summary>
	/// Indicates whether the current <see cref="MessageBuilder"/> instance contains any content, images, or tool calls.
	/// </summary>
	/// <remarks>
	/// The <see cref="HasValue"/> property evaluates to <c>true</c> if:
	/// <lu>
	/// <li>The internal content builder has accumulated text.</li>
	/// <li>There are any tool calls added to the <see cref="MessageBuilder"/>.</li>
	/// <li>There are images included in the <see cref="MessageBuilder"/>.</li>
	/// </lu>
	/// Otherwise, it returns <c>false</c>.
	/// <p>This property is useful for verifying whether the builder contains meaningful data before processing further,
	/// such as converting it to a <see cref="Message"/> or appending additional elements to it.</p>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// var messageBuilder = new MessageBuilder();
	/// Console.WriteLine(messageBuilder.HasValue); // Output: False
	/// // Append a new chunk of content
	/// messageBuilder.Append(new ChatResponseStream
	/// {
	///   Message = new Message
	///   {
	///     Content = "Hello, how can I assist you?",
	///     Role = ChatRole.Assistant
	///   }
	/// });
	/// Console.WriteLine(messageBuilder.HasValue); // Output: True
	/// </code>
	/// </example>
	public bool HasValue => _contentBuilder.Length > 0 || _toolCalls.Any() || _images.Any();
}