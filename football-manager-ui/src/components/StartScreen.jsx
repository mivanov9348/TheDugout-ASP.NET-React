// src/components/StartScreen.jsx
import React from "react";

function StartScreen({ username, onNewGame, onLoadGame }) {
  return (
    <div className="flex flex-col items-center justify-center h-screen bg-gradient-to-b from-gray-800 to-black text-white">
      <h1 className="text-5xl font-bold mb-6">⚽ The Dugout</h1>
      <p className="text-xl mb-10">Welcome, <span className="font-semibold">{username}</span>!</p>

      <div className="space-y-6">
        <button
          onClick={onNewGame}
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
      </div>
    </div>
  );
}

export default StartScreen;
