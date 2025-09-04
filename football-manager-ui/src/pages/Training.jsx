import React, { useState } from 'react';

const Training = () => {
  // Mock players data (replace with backend data later)
  const players = [
    { id: 1, name: 'John Doe', position: 'Forward' },
    { id: 2, name: 'Jane Smith', position: 'Midfielder' },
    { id: 3, name: 'Mike Johnson', position: 'Defender' },
    { id: 4, name: 'Anna Brown', position: 'Goalkeeper' },
  ];

  // Mock skills for dropdown
  const skills = ['Stamina', 'Shooting', 'Passing', 'Defending', 'Dribbling'];

  // Mock training progress data (replace with backend data later)
  const trainingProgress = [
    { id: 1, name: 'John Doe', skill: 'Shooting', efficiency: 5, sessions: 3 },
    { id: 2, name: 'Jane Smith', skill: 'Passing', efficiency: 8, sessions: 5 },
  ];

  const [selectedSkills, setSelectedSkills] = useState({});

  // Handler for skill selection (skeleton, no functionality yet)
  const handleSkillChange = (playerId, skill) => {
    setSelectedSkills((prev) => ({ ...prev, [playerId]: skill }));
  };

  return (
    <div className="min-h-screen bg-gray-100 p-6">
      <div className="max-w-6xl mx-auto">
        <h1 className="text-3xl font-bold text-center mb-8 text-gray-800">Training Management</h1>
        
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
          {/* Left: All Players with Skill Selection */}
          <div>
            <h2 className="text-2xl font-semibold mb-4 text-gray-800">Assign Training</h2>
            <div className="bg-white shadow-md rounded-lg overflow-hidden">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Position</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Skill to Train</th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {players.map((player) => (
                    <tr key={player.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{player.name}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{player.position}</td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <select
                          className="block w-full px-3 py-2 border border-gray-300 bg-white rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                          value={selectedSkills[player.id] || ''}
                          onChange={(e) => handleSkillChange(player.id, e.target.value)}
                        >
                          <option value="">Select Skill</option>
                          {skills.map((skill, index) => (
                            <option key={index} value={skill}>{skill}</option>
                          ))}
                        </select>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <button className="mt-4 px-4 py-2 bg-indigo-600 text-white font-medium rounded-md hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2">
              Save Assignments
            </button>
          </div>

          {/* Right: Training Progress */}
          <div>
            <h2 className="text-2xl font-semibold mb-4 text-gray-800">Training Progress</h2>
            <div className="bg-white shadow-md rounded-lg overflow-hidden">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Skill</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Efficiency Gain</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Sessions</th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {trainingProgress.map((progress) => (
                    <tr key={progress.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{progress.name}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{progress.skill}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{progress.efficiency}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{progress.sessions}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Training;