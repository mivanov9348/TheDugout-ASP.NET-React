import React from "react";

const StartScreen = ({ onNewGame }) => {
  const handleLoadGame = () => {
    const hasSave = false; // TODO: тук после ще закачим реална логика
    if (hasSave) {
      onNewGame();
    } else {
      alert("Няма запазена игра!");
    }
  };

  return (
    <div className="flex flex-col items-center justify-center h-screen bg-gradient-to-b from-gray-800 to-black text-white">
      <h1 className="text-5xl font-bold mb-10">⚽ The Dugout</h1>
      <div className="space-y-6">
        <button
          onClick={onNewGame}
          className="px-8 py-3 bg-green-600 rounded-2xl hover:bg-green-500 transition"
        >
          New Game
        </button>
        <button
          onClick={handleLoadGame}
          className="px-8 py-3 bg-blue-600 rounded-2xl hover:bg-blue-500 transition"
        >
          Load Game
        </button>
      </div>
    </div>
  );
};

export default StartScreen;
