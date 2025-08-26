import React from "react";

const Club = () => {
  const players = [
    { name: "John Smith", position: "GK" },
    { name: "Mark Johnson", position: "DF" },
    { name: "Alex Brown", position: "MF" },
    { name: "David Wilson", position: "FW" },
  ];

  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold mb-6 text-center">🏟️ Club Information</h1>

      {/* Club Overview */}
      <div className="bg-white/80 backdrop-blur-sm rounded-2xl shadow-lg p-6 mb-6">
        <h2 className="text-xl font-semibold mb-4">Club Overview</h2>
        <ul className="text-gray-700 space-y-2">
          <li><span className="font-medium">Name:</span> Example FC</li>
          <li><span className="font-medium">Country:</span> England</li>
          <li><span className="font-medium">League:</span> Premier League</li>
          <li><span className="font-medium">Stadium:</span> Example Arena</li>
          <li><span className="font-medium">Founded:</span> 1905</li>
        </ul>
      </div>

      {/* Players */}
      <div className="bg-white/80 backdrop-blur-sm rounded-2xl shadow-lg p-6 mb-6">
        <h2 className="text-xl font-semibold mb-4">Players</h2>
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
          {players.map((player, index) => (
            <div
              key={index}
              className="bg-gray-100 rounded-xl shadow-md p-3 text-center hover:scale-105 transition-transform duration-200"
            >
              <p className="font-medium text-gray-800">{player.name}</p>
              <p className="text-sm text-gray-500">{player.position}</p>
            </div>
          ))}
        </div>
      </div>

      {/* Extra Info */}
      <div className="bg-white/80 backdrop-blur-sm rounded-2xl shadow-lg p-6">
        <h2 className="text-xl font-semibold mb-4">Extra Info</h2>
        <ul className="text-gray-700 space-y-2">
          <li><span className="font-medium">Manager:</span> Jane Doe</li>
          <li><span className="font-medium">Trophies:</span> 12</li>
          <li><span className="font-medium">Rivals:</span> Rival United</li>
        </ul>
      </div>
    </div>
  );
};

export default Club;
