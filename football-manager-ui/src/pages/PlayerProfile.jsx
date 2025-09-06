// src/pages/PlayerProfile.jsx
import React from 'react';
import { useParams } from 'react-router-dom'; // Added for accessing URL parameters

const PlayerProfile = ({ gameSaveId }) => { // Added prop for gameSaveId if needed later
  const { playerId } = useParams(); // Get playerId from URL, e.g., /player/123

  // Dummy data for testing (in real app, fetch player data based on playerId and gameSaveId)
  const player = {
    id: playerId || 'dummy-id', // Use the URL param for display
    name: 'John Doe',
    age: 28,
    position: 'Forward',
    team: 'Sample FC',
    nationality: 'USA',
    photo: 'https://via.placeholder.com/300x300?text=Player+Photo', // Placeholder image
    attributes: {
      speed: 85,
      strength: 75,
      dribbling: 90,
      shooting: 88,
      passing: 82,
      defense: 60,
    },
    seasonStats: {
      gamesPlayed: 25,
      goals: 15,
      assists: 8,
      yellowCards: 3,
      redCards: 0,
      minutesPlayed: 2200,
    },
  };

  return (
    <div className="min-h-screen bg-gray-100 flex justify-center items-center p-4">
      <div className="bg-white shadow-lg rounded-lg max-w-4xl w-full p-6">
        {/* Header Section */}
        <div className="flex flex-col md:flex-row items-center md:items-start mb-8">
          <img
            src={player.photo}
            alt={`${player.name}'s photo`}
            className="w-48 h-48 rounded-full object-cover mb-4 md:mb-0 md:mr-6 border-4 border-blue-500"
          />
          <div className="text-center md:text-left">
            <h1 className="text-3xl font-bold text-gray-800">{player.name} (ID: {player.id})</h1>
            <p className="text-xl text-gray-600">{player.position} - {player.team}</p>
            <p className="text-lg text-gray-500">Age: {player.age} | Nationality: {player.nationality}</p>
          </div>
        </div>

        {/* Attributes Section */}
        <div className="mb-8">
          <h2 className="text-2xl font-semibold text-gray-800 mb-4">Player Attributes</h2>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
            {Object.entries(player.attributes).map(([key, value]) => (
              <div key={key} className="bg-gray-50 p-4 rounded-md shadow-sm">
                <p className="text-sm text-gray-500 capitalize">{key}</p>
                <p className="text-2xl font-bold text-blue-600">{value}</p>
              </div>
            ))}
          </div>
        </div>

        {/* Season Statistics Section */}
        <div>
          <h2 className="text-2xl font-semibold text-gray-800 mb-4">Season Statistics</h2>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
            {Object.entries(player.seasonStats).map(([key, value]) => (
              <div key={key} className="bg-gray-50 p-4 rounded-md shadow-sm">
                <p className="text-sm text-gray-500 capitalize">{key.replace(/([A-Z])/g, ' $1').trim()}</p>
                <p className="text-2xl font-bold text-green-600">{value}</p>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default PlayerProfile;