import React, { useEffect, useState } from "react";

const Inbox = () => {
  const [messages, setMessages] = useState([]);
  const [selectedMessage, setSelectedMessage] = useState(null);

  // Load ASP NET API
  useEffect(() => {
    fetch("https://localhost:7117/api/inbox")
      .then((res) => res.json())
      .then((data) => setMessages(data));
  }, []);

  const handleSelectMessage = (msg) => {
    setSelectedMessage(msg);

    // if it read
    if (!msg.isRead) {
      fetch(`https://localhost:7117/api/inbox/${msg.id}/read`, {
        method: "POST",
      }).then(() => {
        // local update
        setMessages((prev) =>
          prev.map((m) =>
            m.id === msg.id ? { ...m, isRead: true } : m
          )
        );
      });
    }
  };

  return (
    <div className="flex h-full">
      {/* Sidebar */}
      <div className="w-1/3 bg-blue-50 border-r overflow-y-auto">
        <h2 className="text-lg font-bold p-4">Inbox</h2>
        <ul>
          {messages.map((msg) => (
            <li
              key={msg.id}
              onClick={() => handleSelectMessage(msg)}
              className={`cursor-pointer px-4 py-2 border-b hover:bg-blue-100 ${
                msg.isRead ? "text-slate-500" : "font-bold text-black"
              }`}
            >
              <div>{msg.subject}</div>
              <div className="text-xs text-slate-400">
                {new Date(msg.date).toLocaleDateString()}
              </div>
            </li>
          ))}
        </ul>
      </div>

      {/* Content */}
      <div className="flex-1 p-6">
        {selectedMessage ? (
          <>
            <h3 className="text-xl font-bold mb-2">{selectedMessage.subject}</h3>
            <p className="text-sm text-slate-400 mb-4">
              {new Date(selectedMessage.date).toLocaleString()}
            </p>
            <p>{selectedMessage.body}</p>
          </>
        ) : (
          <p className="text-slate-400">Select a message to read</p>
        )}
      </div>
    </div>
  );
};

export default Inbox;
