// src/components/StartScreen.jsx
import React from "react";
import Swal from "sweetalert2";

function StartScreen({ username, onNewGame, onLoadGame, onLogout }) {
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
        onNewGame();
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
        >
          New Game
        </button>
        <button
          onClick={onLoadGame}
          className="px-8 py-3 bg-blue-600 rounded-2xl hover:bg-blue-500 transition"
        >
          Load Game
        </button>
        <button
          onClick={onLogout}
          className="px-8 py-3 bg-red-600 rounded-2xl hover:bg-red-500 transition"
        >
          Logout
        </button>
      </div>
    </div>
  );
}

export default StartScreen;
