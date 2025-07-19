import { useState } from 'react';
import ReactMarkdown from 'react-markdown'; // 👈 Add this
import './App.css';

function App() {
  const [open, setOpen] = useState(false);
  const [messages, setMessages] = useState([
    { from: 'bot', text: 'Hello! I am your Feature Agent Assistant 🤖. How can I help?' },
  ]);
  const [input, setInput] = useState('');

  const handleSend = async () => {
    if (!input.trim()) return;

    const userMessage = { from: 'user', text: input };
    setMessages((prev) => [...prev, userMessage]);

    try {
      const res = await fetch('http://localhost:5000/generate-feature', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          business: input,
          implementation: 'To be defined...',
          quality: 'To be verified...',
        }),
      });

      const data = await res.json();

      // 👇 fetch .md file content
      const fileRes = await fetch(`${data.download}`);
      const markdown = await fileRes.text();

      const botMessage = {
        from: 'bot',
        text: `✅ Feature saved! [📥 Download feature.md](${data.download})`,
      };

      setMessages((prev) => [...prev, botMessage]);
      setInput('');
    } catch (err) {
      console.error(err);
      setMessages((prev) => [
        ...prev,
        { from: 'bot', text: '❌ Failed to generate feature file.' },
      ]);
    }
  };

  return (
    <div className="container">
      {!open && (
        <button className="bot-button" onClick={() => setOpen(true)}>
          🤖
        </button>
      )}
      {open && (
        <div className="chat-box">
          <div className="chat-header">
            Feature Agent Assistant
            <button className="close-button" onClick={() => setOpen(false)}>
              ✖️
            </button>
          </div>
          <div className="chat-body">
            {messages.map((msg, i) => (
    <div key={i} className={`msg ${msg.from}`}>
      {msg.from === 'bot' ? (
        <ReactMarkdown>{msg.text}</ReactMarkdown>
      ) : (
        <span>{msg.text}</span>
      )}
  </div>
))}
          </div>
          <div className="chat-input">
            <input
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleSend()}
              placeholder="Type your business context..."
            />
            <button onClick={handleSend}>Send</button>
          </div>
        </div>
      )}
    </div>
  );
}

export default App;
