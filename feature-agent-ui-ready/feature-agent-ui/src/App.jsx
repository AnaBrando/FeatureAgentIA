// App.jsx
import { useState } from 'react';
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
    setInput('');

    // Call your backend here (mocked below)
    const res = await fetch('http://localhost:5000/generate-feature', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ business: input, implementation: '', quality: '' }),
    });
    const data = await res.json();
    setMessages((prev) => [...prev, { from: 'bot', text: `✅ Feature saved at: ${data.path}` }]);
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
              <div key={i} className={`msg ${msg.from}`}>{msg.text}</div>
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
