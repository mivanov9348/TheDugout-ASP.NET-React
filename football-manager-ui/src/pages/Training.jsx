import React, { useEffect, useState } from 'react';

// TrainingComponent.jsx
// Single-file React component (Tailwind CSS classes used) that replaces the original table UI
// with a card-based, colorful, animated Training Ground UI. Stores mock progress in localStorage
// so you can iterate without backend. Copy this file into your React app and render <Training />.

const MOCK_PLAYERS = [
  { id: 1, name: 'John Doe', position: 'Forward', avatarColor: 'from-red-400 to-red-600' },
  { id: 2, name: 'Jane Smith', position: 'Midfielder', avatarColor: 'from-green-400 to-green-600' },
  { id: 3, name: 'Mike Johnson', position: 'Defender', avatarColor: 'from-blue-400 to-blue-600' },
  { id: 4, name: 'Anna Brown', position: 'Goalkeeper', avatarColor: 'from-yellow-300 to-yellow-500' },
];

const SKILLS = ['Stamina', 'Shooting', 'Passing', 'Defending', 'Dribbling'];

const STORAGE_KEYS = {
  progress: 'tm_training_progress_v1',
  lastTrainedAt: 'tm_last_trained_at_v1',
};

function clamp(v, a = 0, b = 100) {
  return Math.max(a, Math.min(b, Math.round(v)));
}

function isSameDay(d1, d2) {
  if (!d1 || !d2) return false;
  const a = new Date(d1);
  const b = new Date(d2);
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
}

export default function Training() {
  // selected skill per player (local UI state)
  const [selectedSkills, setSelectedSkills] = useState({});

  // training progress is an array of { playerId, skill, efficiency (0-100), sessions, lastTrained }
  const [trainingProgress, setTrainingProgress] = useState([]);

  // last time we ran a full-team training (ISO string) to enforce daily training
  const [lastTrainedAt, setLastTrainedAt] = useState(null);

  const [message, setMessage] = useState(null);

  useEffect(() => {
    // hydrate from localStorage
    try {
      const p = localStorage.getItem(STORAGE_KEYS.progress);
      const l = localStorage.getItem(STORAGE_KEYS.lastTrainedAt);
      if (p) setTrainingProgress(JSON.parse(p));
      if (l) setLastTrainedAt(l);
    } catch (e) {
      console.warn('Could not load training data', e);
    }
  }, []);

  useEffect(() => {
    // persist
    try {
      localStorage.setItem(STORAGE_KEYS.progress, JSON.stringify(trainingProgress));
    } catch (e) {}
  }, [trainingProgress]);

  useEffect(() => {
    try {
      if (lastTrainedAt) localStorage.setItem(STORAGE_KEYS.lastTrainedAt, lastTrainedAt);
    } catch (e) {}
  }, [lastTrainedAt]);

  const handleSkillChange = (playerId, skill) => {
    setSelectedSkills((prev) => ({ ...prev, [playerId]: skill }));
  };

  const alreadyTrainedToday = lastTrainedAt && isSameDay(lastTrainedAt, new Date().toISOString());

  const estimateEfficiencyGain = (skill, position) => {
    // small heuristic for mock: some positions gain faster for certain skills
    const base = {
      Stamina: 6,
      Shooting: 5,
      Passing: 5,
      Defending: 5,
      Dribbling: 5,
    }[skill] || 4;

    const positionBoost = {
      Forward: skill === 'Shooting' ? 3 : skill === 'Dribbling' ? 2 : 0,
      Midfielder: skill === 'Passing' ? 3 : skill === 'Stamina' ? 1 : 0,
      Defender: skill === 'Defending' ? 3 : 0,
      Goalkeeper: skill === 'Stamina' ? 2 : 0,
    }[position] || 0;

    // add a small random factor
    const randomness = Math.floor(Math.random() * 4); // 0-3
    return base + positionBoost + randomness;
  };

  const handleSaveAssignments = () => {
    // if no assignments -> prompt
    const assigned = Object.keys(selectedSkills).filter((k) => selectedSkills[k]);
    if (assigned.length === 0) {
      flashMessage('Select at least one skill to train', 'warning');
      return;
    }

    if (alreadyTrainedToday) {
      flashMessage('Team was already trained today. Come back tomorrow.', 'error');
      return;
    }

    // perform mock training: update or create progress entries
    const now = new Date().toISOString();
    const updated = [...trainingProgress];

    assigned.forEach((playerIdStr) => {
      const playerId = Number(playerIdStr);
      const player = MOCK_PLAYERS.find((p) => p.id === playerId);
      const skill = selectedSkills[playerId];
      if (!player || !skill) return;

      const gain = estimateEfficiencyGain(skill, player.position);

      const existingIndex = updated.findIndex((t) => t.playerId === playerId && t.skill === skill);
      if (existingIndex >= 0) {
        const entry = { ...updated[existingIndex] };
        entry.efficiency = clamp(entry.efficiency + gain, 0, 100);
        entry.sessions = entry.sessions + 1;
        entry.lastTrained = now;
        updated[existingIndex] = entry;
      } else {
        updated.push({
          playerId,
          playerName: player.name,
          skill,
          efficiency: clamp(20 + gain, 0, 100), // starting baseline 20
          sessions: 1,
          lastTrained: now,
        });
      }
    });

    setTrainingProgress(sortProgress(updated));
    setLastTrainedAt(now);
    flashMessage('Training finished — gains applied! ⚽', 'success');
    // reset selections
    setSelectedSkills({});
  };

  const sortProgress = (arr) => {
    return arr.sort((a, b) => b.efficiency - a.efficiency);
  };

  function flashMessage(text, type = 'info') {
    setMessage({ text, type });
    setTimeout(() => setMessage(null), 3000);
  }

  const nextAvailableInfo = () => {
    if (!lastTrainedAt) return 'Available now';
    const last = new Date(lastTrainedAt);
    const next = new Date(last.getTime() + 24 * 60 * 60 * 1000);
    if (new Date() >= next) return 'Available now';
    const diff = next - new Date();
    const hours = Math.floor(diff / (1000 * 60 * 60));
    const mins = Math.floor((diff - hours * 1000 * 60 * 60) / (1000 * 60));
    return `Next training in ${hours}h ${mins}m`;
  };

  return (
    <div className="min-h-screen p-6 bg-gradient-to-br from-gray-100 to-gray-200">
      <div className="max-w-6xl mx-auto">
        <header className="mb-8">
          <h1 className="text-4xl font-extrabold text-center text-indigo-700 drop-shadow-md">Training Ground ⚽</h1>
          <p className="text-center text-sm text-gray-600 mt-2">Assign skills and run a daily training simulation. No backend needed — mock data persists in localStorage.</p>
        </header>

        <div className="flex gap-6 flex-col lg:flex-row">
          {/* Left column - players list */}
          <section className="flex-1">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-2xl font-semibold text-gray-800">Players</h2>
              <div className="text-sm text-gray-600">{nextAvailableInfo()}</div>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              {MOCK_PLAYERS.map((player) => (
                <article key={player.id} className="bg-white rounded-2xl shadow-lg p-4 transform transition hover:-translate-y-1 hover:shadow-2xl">
                  <div className="flex items-center gap-4">
                    <div className={`w-14 h-14 rounded-full bg-gradient-to-br ${player.avatarColor} flex items-center justify-center text-white font-bold text-lg shrink-0`}>{player.name.split(' ').map(n=>n[0]).slice(0,2).join('')}</div>

                    <div className="flex-1">
                      <div className="flex items-center justify-between">
                        <div>
                          <h3 className="text-lg font-semibold text-gray-900">{player.name}</h3>
                          <p className="text-sm text-gray-500">{player.position}</p>
                        </div>
                        <div className="text-xs px-2 py-1 rounded-full bg-gray-100 text-gray-600">ID #{player.id}</div>
                      </div>

                      <div className="mt-3">
                        <label className="block text-xs font-medium text-gray-600 mb-1">Skill to train</label>
                        <select
                          className="block w-full px-3 py-2 border border-gray-200 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-indigo-400"
                          value={selectedSkills[player.id] || ''}
                          onChange={(e) => handleSkillChange(player.id, e.target.value)}
                        >
                          <option value="">— Select Skill —</option>
                          {SKILLS.map((s) => (
                            <option key={s} value={s}>{s}</option>
                          ))}
                        </select>
                      </div>
                    </div>
                  </div>
                </article>
              ))}
            </div>

            <div className="mt-4 flex items-center gap-3">
              <button
                onClick={handleSaveAssignments}
                className={`px-5 py-3 bg-gradient-to-r from-indigo-600 to-purple-600 text-white rounded-xl shadow-md font-semibold transition transform hover:scale-[1.02] focus:ring-4 focus:ring-indigo-200 disabled:opacity-60`}
              >
                Run Training
              </button>

              <button
                onClick={() => { setSelectedSkills({}); flashMessage('Selections cleared'); }}
                className="px-4 py-2 bg-white border border-gray-200 rounded-lg text-gray-700 shadow-sm"
              >
                Clear
              </button>

              <div className="ml-auto text-sm text-gray-600">Tip: you can train multiple players at once</div>
            </div>
          </section>

          {/* Right column - training progress */}
          <aside className="w-full lg:w-1/2">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-2xl font-semibold text-gray-800">Training Progress</h2>
              <div className="text-sm text-gray-500">Saved locally</div>
            </div>

            <div className="space-y-3">
              {trainingProgress.length === 0 && (
                <div className="bg-white rounded-2xl shadow-inner p-6 text-center text-gray-600">No progress yet — run a training to start improving players.</div>
              )}

              {trainingProgress.map((p) => (
                <div key={`${p.playerId}-${p.skill}`} className="bg-white rounded-2xl shadow p-4 flex items-center gap-4">
                  <div className="w-12 h-12 rounded-full bg-gradient-to-br from-gray-300 to-gray-400 flex items-center justify-center text-white font-semibold">
                    {p.playerName.split(' ').map(n=>n[0]).slice(0,2).join('')}
                  </div>

                  <div className="flex-1">
                    <div className="flex items-center justify-between">
                      <div>
                        <div className="text-sm font-semibold text-gray-900">{p.playerName}</div>
                        <div className="text-xs text-gray-500">{p.skill} · {p.sessions} session{p.sessions > 1 ? 's' : ''}</div>
                      </div>
                      <div className="text-sm font-medium text-gray-700">{p.efficiency}%</div>
                    </div>

                    <div className="mt-3">
                      {/* Progress bar */}
                      <div className="w-full bg-gray-100 rounded-full h-3">
                        <div className="h-3 rounded-full bg-gradient-to-r from-green-400 to-green-600" style={{ width: `${clamp(p.efficiency, 0, 100)}%` }} />
                      </div>

                      <div className="mt-2 text-xs text-gray-400">Last trained: {new Date(p.lastTrained).toLocaleString()}</div>
                    </div>
                  </div>
                </div>
              ))}
            </div>

            {/* compact history chart (mock) */}
            <div className="mt-6 bg-white p-4 rounded-2xl shadow">
              <h3 className="text-sm font-semibold text-gray-800 mb-2">Quick Summary</h3>
              <div className="grid grid-cols-2 gap-3">
                <div className="p-3 rounded-lg bg-gray-50 text-center">
                  <div className="text-xs text-gray-500">Total sessions</div>
                  <div className="text-lg font-bold text-gray-800">{trainingProgress.reduce((s, x) => s + x.sessions, 0)}</div>
                </div>
                <div className="p-3 rounded-lg bg-gray-50 text-center">
                  <div className="text-xs text-gray-500">Tracked entries</div>
                  <div className="text-lg font-bold text-gray-800">{trainingProgress.length}</div>
                </div>
              </div>

              <div className="mt-3 text-xs text-gray-500">Last full training: {lastTrainedAt ? new Date(lastTrainedAt).toLocaleString() : 'never'}</div>
            </div>
          </aside>
        </div>

        {/* notification */}
        {message && (
          <div className={`fixed right-6 bottom-6 p-3 rounded-xl shadow-lg ${message.type === 'success' ? 'bg-green-50 border border-green-200' : message.type === 'error' ? 'bg-red-50 border border-red-200' : 'bg-white border border-gray-200'}`}>
            <div className="text-sm text-gray-800">{message.text}</div>
          </div>
        )}

        <footer className="mt-10 text-center text-xs text-gray-500">Prototype — no backend yet. When you add an API we can swap localStorage for real endpoints.</footer>
      </div>
    </div>
  );
}
