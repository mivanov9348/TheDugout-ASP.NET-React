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

  if (!gameSaveId)
    return (
      <div className="p-6 text-gray-400 bg-gradient-to-br from-gray-900 to-gray-800 h-screen flex items-center justify-center">
        Loading...
      </div>
    );

  return (
    <div className="flex h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900 text-gray-100">
      {/* Sidebar */}
      {sidebarOpen && (
        <div className="w-80 bg-gray-800/50 backdrop-blur-md border-r border-gray-700 shadow-xl flex flex-col transition-all duration-300">
          <div className="flex items-center justify-between p-4 border-b border-gray-700 bg-gray-900/70 sticky top-0 z-10">
            <h2 className="text-lg font-bold text-gray-100 flex items-center gap-2">
              <Mail className="w-5 h-5 text-blue-400" /> Inbox
            </h2>
            <button
              className="md:hidden text-gray-400 hover:text-gray-200"
              onClick={() => setSidebarOpen(false)}
              aria-label="Close sidebar"
            >
              <ChevronLeft className="w-5 h-5" />
            </button>
          </div>

          <ul className="flex-1 overflow-y-auto scrollbar-thin scrollbar-thumb-gray-700 scrollbar-track-gray-900">
            {loading && (
              <li className="p-4 text-sm text-gray-500 text-center">Зареждане...</li>
            )}

            {messages.map((msg) => (
              <li
                key={msg.id}
                onClick={() => handleSelectMessage(msg)}
                className={`px-4 py-3 border-b border-gray-700 hover:bg-gray-800/70 transition-all duration-200 cursor-pointer flex justify-between items-center ${
                  selectedMessage?.id === msg.id ? "bg-gray-800/90 border-gray-600" : ""
                }`}
              >
                <div className="flex-1">
                  <div className="flex justify-between items-center">
                    <span
                      className={`truncate ${
                        msg.isRead
                          ? "text-gray-400"
                          : "font-semibold text-white"
                      }`}
                    >
                      {msg.subject}
                    </span>
                    {!msg.isRead && (
                      <span className="w-2 h-2 bg-blue-400 rounded-full ml-2 shadow-[0_0_6px_#3b82f6]" />
                    )}
                  </div>
                  <div className="text-xs text-gray-500">
                    {new Date(msg.date).toLocaleDateString()}
                  </div>
                </div>

                <button
                  onClick={(e) => {
                    e.stopPropagation();
                    if (!window.confirm("Сигурни ли сте, че искате да изтриете съобщението?")) return;
                    handleDeleteMessage(msg.id);
                  }}
                  className="ml-3 text-gray-500 hover:text-red-500 transition duration-150"
                  aria-label={`Delete message ${msg.subject}`}
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </li>
            ))}

            {messages.length === 0 && !loading && (
              <li className="p-4 text-sm text-gray-500 text-center">Няма съобщения</li>
            )}
          </ul>
        </div>
      )}

      {/* Message content */}
      <div className="flex-1 p-10 overflow-y-auto relative">
        {selectedMessage ? (
          <div className="max-w-3xl mx-auto bg-gray-800/40 backdrop-blur-md border border-gray-700 rounded-2xl shadow-xl p-8 transition-all duration-300 hover:border-gray-600 hover:shadow-2xl">
            <h3 className="text-2xl font-bold mb-2 flex items-center gap-3 text-blue-300">
              {selectedMessage.isRead ? (
                <MailOpen className="w-6 h-6 text-gray-400" />
              ) : (
                <Mail className="w-6 h-6 text-blue-400" />
              )}
              {selectedMessage.subject}
            </h3>
            <p className="text-xs text-gray-500 mb-6 border-b border-gray-700 pb-2">
              {new Date(selectedMessage.date).toLocaleString()}
            </p>
            <div className="text-gray-200 leading-relaxed whitespace-pre-line tracking-wide">
              {selectedMessage.body}
            </div>
          </div>
        ) : (
          <div className="h-full flex items-center justify-center text-gray-500 text-sm italic">
            Choose a message from the inbox to read it.
          </div>
        )}
      </div>
    </div>
  );
};

export default Inbox;
