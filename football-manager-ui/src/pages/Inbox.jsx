import React, { useEffect, useState } from "react";
import { Mail, MailOpen, ChevronLeft } from "lucide-react";

const Inbox = ({ gameSaveId }) => {
  const [messages, setMessages] = useState([]);
  const [selectedMessage, setSelectedMessage] = useState(null);
  const [sidebarOpen, setSidebarOpen] = useState(true);

  useEffect(() => {
    if (!gameSaveId) return;

    const fetchMessages = async () => {
      try {
        const token = localStorage.getItem("token");
        const res = await fetch(`/api/inbox?gameSaveId=${gameSaveId}`, {
          headers: { Authorization: `Bearer ${token}` },
        });
        if (!res.ok) throw new Error("Failed to fetch messages");
        const data = await res.json();
        setMessages(data);
      } catch (err) {
        console.error("Error fetching inbox messages:", err);
      }
    };

    fetchMessages();
  }, [gameSaveId]);

  const handleSelectMessage = (msg) => {
    setSelectedMessage(msg);

    if (!msg.isRead) {
      const token = localStorage.getItem("token");
      fetch(`/api/inbox/${msg.id}/read?gameSaveId=${gameSaveId}`, {
        method: "POST",
        headers: { Authorization: `Bearer ${token}` },
      }).then(() => {
        setMessages((prev) =>
          prev.map((m) => (m.id === msg.id ? { ...m, isRead: true } : m))
        );
      });
    }
  };

  if (!gameSaveId) return <div className="p-6 text-gray-500">Зареждане...</div>;

  return (
    <div className="flex h-screen bg-gray-50">
      {/* Sidebar */}
      {sidebarOpen && (
        <div className="w-80 bg-white border-r shadow-sm flex flex-col">
          <div className="flex items-center justify-between p-4 border-b">
            <h2 className="text-lg font-bold text-gray-800 flex items-center gap-2">
              <Mail className="w-5 h-5 text-blue-500" /> Inbox
            </h2>
            <button
              className="md:hidden text-gray-500"
              onClick={() => setSidebarOpen(false)}
            >
              <ChevronLeft className="w-5 h-5" />
            </button>
          </div>

          <ul className="flex-1 overflow-y-auto">
            {messages.map((msg) => (
              <li
                key={msg.id}
                onClick={() => handleSelectMessage(msg)}
                className={`cursor-pointer px-4 py-3 border-b hover:bg-blue-50 transition ${
                  selectedMessage?.id === msg.id ? "bg-blue-100" : ""
                }`}
              >
                <div className="flex justify-between items-center">
                  <span
                    className={`truncate ${
                      msg.isRead
                        ? "text-gray-500"
                        : "font-semibold text-gray-900"
                    }`}
                  >
                    {msg.subject}
                  </span>
                  {!msg.isRead && (
                    <span className="w-2 h-2 bg-blue-500 rounded-full"></span>
                  )}
                </div>
                <div className="text-xs text-gray-400">
                  {new Date(msg.date).toLocaleDateString()}
                </div>
              </li>
            ))}
            {messages.length === 0 && (
              <li className="p-4 text-sm text-gray-400 text-center">
                No messages
              </li>
            )}
          </ul>
        </div>
      )}

      {/* Content */}
      <div className="flex-1 p-6 overflow-y-auto">
        {selectedMessage ? (
          <div className="max-w-3xl mx-auto bg-white shadow-sm rounded-lg p-6">
            <h3 className="text-2xl font-bold text-gray-900 mb-2 flex items-center gap-2">
              {selectedMessage.isRead ? (
                <MailOpen className="w-5 h-5 text-gray-400" />
              ) : (
                <Mail className="w-5 h-5 text-blue-500" />
              )}
              {selectedMessage.subject}
            </h3>
            <p className="text-sm text-gray-500 mb-4">
              {new Date(selectedMessage.date).toLocaleString()}
            </p>
            <hr className="mb-4" />
            <p className="text-gray-800 leading-relaxed whitespace-pre-line">
              {selectedMessage.body}
            </p>
          </div>
        ) : (
          <div className="h-full flex items-center justify-center text-gray-400 text-sm">
            Select a message to read
          </div>
        )}
      </div>
    </div>
  );
};

export default Inbox;
