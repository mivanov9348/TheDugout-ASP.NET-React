import React, { useState } from "react";
import Swal from "sweetalert2";
import { useNavigate } from "react-router-dom";

function StartScreen({ username, onNewGame, onLoadGame, onLogout, isAdmin }) {
  const [loading, setLoading] = useState(false);
  const [loadingMessage, setLoadingMessage] = useState("");
  const navigate = useNavigate();

  // 🔹 Засега твърдо — после ще идва от бекенда

  const confirmNewGame = () => {
    Swal.fire({
      title: "Започни нова игра?",
      text: "Сигурен ли си?",
      icon: "question",
      showCancelButton: true,
      confirmButtonText: "Да",
      cancelButtonText: "Не",
    }).then((result) => {
      if (result.isConfirmed) {
        setLoading(true);
        setLoadingMessage("Стартиране на нова игра...");

        onNewGame((msg) => setLoadingMessage(msg)).finally(() => {
          setLoading(false);
        });
      }
    });
  };

  return (
    <div className="flex flex-col items-center justify-center h-screen bg-gradient-to-b from-gray-800 to-black text-white">
      <h1 className="text-5xl font-bold mb-6">⚽ The Dugout</h1>
      <p className="text-xl mb-10">
        Welcome, <span className="font-semibold">{username}</span>!
      </p>

      <div className="space-y-6 flex flex-col">
        <button
          onClick={confirmNewGame}
          className="px-8 py-3 bg-green-600 rounded-2xl hover:bg-green-500 transition"
          disabled={loading}
        >
          New Game
        </button>
        <button
          onClick={onLoadGame}
          className="px-8 py-3 bg-blue-600 rounded-2xl hover:bg-blue-500 transition"
          disabled={loading}
        >
          Load Game
        </button>

        {/* 🔹 Покажи бутона само ако е админ */}
        {isAdmin && (
          <button
            onClick={() => navigate("/admin")}
            className="px-8 py-3 bg-yellow-500 rounded-2xl hover:bg-yellow-400 transition"
          >
            Admin Panel
          </button>
        )}

        <button
          onClick={onLogout}
          className="px-8 py-3 bg-red-600 rounded-2xl hover:bg-red-500 transition"
          disabled={loading}
        >
          Logout
        </button>
      </div>

      {loading && (
        <div className="mt-8 flex flex-col items-center space-y-3">
          <div className="w-64 bg-gray-700 rounded-full h-4">
            <div
              className="bg-green-500 h-4 rounded-full animate-pulse"
              style={{ width: "70%" }}
            />
          </div>
          <p className="text-sm text-gray-300">{loadingMessage}</p>
        </div>
      )}
    </div>
  );
}

export default StartScreen;
