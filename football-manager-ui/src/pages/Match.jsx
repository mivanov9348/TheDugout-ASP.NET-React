import { useState, useEffect } from "react";

// Mock данни
const mockHomeTeam = {
  name: "FC HomeTown",
  players: [
    { number: 1, position: "GK", name: "John Keeper", stats: { goals: 0, passes: 20 } },
    { number: 5, position: "DF", name: "Alex Stone", stats: { goals: 0, passes: 15 } },
    { number: 10, position: "MF", name: "Chris Playmaker", stats: { goals: 1, passes: 32 } },
    { number: 9, position: "FW", name: "Mark Striker", stats: { goals: 2, passes: 8 } },
  ],
};

const mockAwayTeam = {
  name: "United Guests",
  players: [
    { number: 1, position: "GK", name: "Peter Gloves", stats: { goals: 0, passes: 18 } },
    { number: 4, position: "DF", name: "Robert Wall", stats: { goals: 0, passes: 10 } },
    { number: 8, position: "MF", name: "Sam Runner", stats: { goals: 0, passes: 25 } },
    { number: 11, position: "FW", name: "Leo Shooter", stats: { goals: 1, passes: 6 } },
  ],
};

const mockEvents = [
  { minute: 5, text: "⚽ Goal! Mark Striker scores for FC HomeTown" },
  { minute: 18, text: "🟨 Yellow card for Robert Wall" },
  { minute: 29, text: "🔥 Chris Playmaker creates a big chance" },
  { minute: 40, text: "⚽ Goal! Leo Shooter equalizes" },
  { minute: 67, text: "⚽ Goal! Mark Striker again! 2-1" },
];

export default function MatchPage() {
  const [events, setEvents] = useState([]);

  useEffect(() => {
    // Симулираме live commentary
    let i = 0;
    const interval = setInterval(() => {
      if (i < mockEvents.length) {
        setEvents((prev) => [...prev, mockEvents[i]]);
        i++;
      } else {
        clearInterval(interval);
      }
    }, 2000);
    return () => clearInterval(interval);
  }, []);

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-900 to-black text-white p-4 flex flex-col">
      {/* Scoreboard */}
      <div className="w-full flex justify-between items-center bg-gray-800 rounded-2xl shadow-lg p-4 text-2xl font-bold">
        <span>{mockHomeTeam.name}</span>
        <span className="text-4xl">2 : 1</span>
        <span>{mockAwayTeam.name}</span>
      </div>

      {/* Main Content */}
      <div className="flex flex-1 mt-4 gap-4">
        {/* Home Team */}
        <div className="w-1/4 bg-gray-800 rounded-xl p-3 overflow-y-auto shadow-lg">
          <h2 className="text-center font-bold mb-2">{mockHomeTeam.name}</h2>
          <table className="w-full text-sm">
            <thead>
              <tr className="text-gray-400 border-b border-gray-700">
                <th>#</th>
                <th>Pos</th>
                <th>Name</th>
                <th>G</th>
                <th>P</th>
              </tr>
            </thead>
            <tbody>
              {mockHomeTeam.players.map((p) => (
                <tr key={p.number} className="border-b border-gray-700">
                  <td>{p.number}</td>
                  <td>{p.position}</td>
                  <td>{p.name}</td>
                  <td className="text-center">{p.stats.goals}</td>
                  <td className="text-center">{p.stats.passes}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Commentary */}
        <div className="w-2/4 bg-gray-900 rounded-xl p-4 flex flex-col shadow-inner">
          <h2 className="text-center font-bold text-xl mb-2">Live Commentary</h2>
          <div className="flex-1 overflow-y-auto space-y-2">
            {events.map((e, i) => (
              <div key={i} className="bg-gray-800 p-2 rounded-lg shadow-md animate-fadeIn">
                <span className="font-bold text-green-400">{e.minute}'</span> {e.text}
              </div>
            ))}
          </div>
        </div>

        {/* Away Team */}
        <div className="w-1/4 bg-gray-800 rounded-xl p-3 overflow-y-auto shadow-lg">
          <h2 className="text-center font-bold mb-2">{mockAwayTeam.name}</h2>
          <table className="w-full text-sm">
            <thead>
              <tr className="text-gray-400 border-b border-gray-700">
                <th>#</th>
                <th>Pos</th>
                <th>Name</th>
                <th>G</th>
                <th>P</th>
              </tr>
            </thead>
            <tbody>
              {mockAwayTeam.players.map((p) => (
                <tr key={p.number} className="border-b border-gray-700">
                  <td>{p.number}</td>
                  <td>{p.position}</td>
                  <td>{p.name}</td>
                  <td className="text-center">{p.stats.goals}</td>
                  <td className="text-center">{p.stats.passes}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
