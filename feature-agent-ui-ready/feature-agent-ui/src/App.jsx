import { useState } from 'react';
import ReactMarkdown from 'react-markdown';
import './App.css';

function App() {
  const [open, setOpen] = useState(false);
  const [messages, setMessages] = useState([
    { from: 'bot', text: 'Hello! I am your Feature Agent Assistant 🤖. How can I help?' },
  ]);
  const [file, setFile] = useState(null);
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false); // 👈 NEW

  const handleSend = async () => {
    if (file === null) return;

    const userMessage = { from: 'user', text: input };
    setMessages((prev) => [...prev, userMessage]);

    const formData = new FormData();
    if (file) {
      formData.append('file', file);
    } else {
      formData.append('business', input);
      formData.append('implementation', 'To be defined...');
      formData.append('quality', 'To be verified...');
    }

    setLoading(true); // 👈 START loading
    try {
      const res = await fetch('http://localhost:5000/generate-feature', {
        method: 'POST',
        body: formData
      });

      const data = await res.json();

      const fileRes = await fetch(`${data.download}`);
      const markdown = await fileRes.text(); // not used but fetched

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
    } finally {
      setLoading(false); // 👈 STOP loading
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
            <input type="file" onChange={(e) => setFile(e.target.files[0])} />
            {loading ? (
              <div className="send-loading-indicator" aria-label="Loading" />
            ) : (
              <button onClick={handleSend}>Send</button>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

export default App;
