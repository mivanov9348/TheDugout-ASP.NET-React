// Frontend: Inbox.jsx
// React component using Tailwind and lucide-react. Put this file in your React project (e.g. src/components/Inbox.jsx)

import React, { useEffect, useState } from "react";
import { Mail, MailOpen, ChevronLeft, Trash2 } from "lucide-react";

const Inbox = ({ gameSaveId }) => {
  const [messages, setMessages] = useState([]);
  const [selectedMessage, setSelectedMessage] = useState(null);
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!gameSaveId) return;

    const fetchMessages = async () => {
      setLoading(true);
      try {
        const token = localStorage.getItem("token");
        const res = await fetch(`/api/inbox?gameSaveId=${gameSaveId}`, {
          headers: token ? { Authorization: `Bearer ${token}` } : {},
        });
        if (!res.ok) throw new Error("Failed to fetch messages");
        const data = await res.json();
        setMessages(data);
      } catch (err) {
        console.error("Error fetching inbox messages:", err);
      } finally {
        setLoading(false);
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
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      })
        .then((res) => {
          if (!res.ok) throw new Error("Failed to mark read");
          setMessages((prev) =>
            prev.map((m) => (m.id === msg.id ? { ...m, isRead: true } : m))
          );
          setSelectedMessage((s) => (s && s.id === msg.id ? { ...s, isRead: true } : s));
        })
        .catch((err) => console.error("Error marking message as read:", err));
    }
  };

  const handleDeleteMessage = async (id) => {
    try {
      const token = localStorage.getItem("token");
      const res = await fetch(`/api/inbox/${id}?gameSaveId=${gameSaveId}`, {
        method: "DELETE",
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      });
      if (!res.ok) throw new Error("Failed to delete message");

      setMessages((prev) => prev.filter((m) => m.id !== id));
      if (selectedMessage?.id === id) setSelectedMessage(null);
    } catch (err) {
      console.error("Error deleting message:", err);
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
              aria-label="Close sidebar"
            >
              <ChevronLeft className="w-5 h-5" />
            </button>
          </div>

          <ul className="flex-1 overflow-y-auto">
            {loading && (
              <li className="p-4 text-sm text-gray-400 text-center">Зареждане...</li>
            )}

            {messages.map((msg) => (
              <li
                key={msg.id}
                className={`px-4 py-3 border-b hover:bg-blue-50 transition flex justify-between items-center ${
                  selectedMessage?.id === msg.id ? "bg-blue-100" : ""
                }`}
              >
                <div
                  onClick={() => handleSelectMessage(msg)}
                  className="flex-1 cursor-pointer"
                  role="button"
                >
                  <div className="flex justify-between items-center">
                    <span
                      className={`truncate ${
                        msg.isRead ? "text-gray-500" : "font-semibold text-gray-900"
                      }`}
                    >
                      {msg.subject}
                    </span>
                    {!msg.isRead && (
                      <span className="w-2 h-2 bg-blue-500 rounded-full ml-2"></span>
                    )}
                  </div>
                  <div className="text-xs text-gray-400">
                    {new Date(msg.date).toLocaleDateString()}
                  </div>
                </div>

                {/* Delete button */}
                <button
                  onClick={(e) => {
                    e.stopPropagation();
                    // simple confirm (optional) - comment out if you don't want confirmation
                    if (!window.confirm("Сигурни ли сте че искате да изтриете съобщението?")) return;
                    handleDeleteMessage(msg.id);
                  }}
                  className="ml-3 text-gray-400 hover:text-red-600 p-1"
                  aria-label={`Delete message ${msg.subject}`}
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </li>
            ))}

            {messages.length === 0 && !loading && (
              <li className="p-4 text-sm text-gray-400 text-center">No messages</li>
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

