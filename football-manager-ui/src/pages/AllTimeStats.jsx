import { useState, useEffect } from "react";
import { 
  BarChart, Trophy, Goal, Shield, Percent, Star, Award, Users 
} from "lucide-react";

const getMockData = () => ({
  managerStats: {
    totalSeasons: 3,
    trophiesWon: 2,
    totalMatches: 114,
    totalWins: 70,
    totalDraws: 24,
    totalLosses: 20,
    goalsScored: 210,
    goalsConceded: 95,
    winPercentage: 61.4,
  },
  leagueChampions: [
    { season: "2024/25", team: "Твоят Отбор" },
    { season: "2023/24", team: "Лудогорец" },
    { season: "2022/23", team: "ЦСКА - София" },
  ],
  cupWinners: [
    { season: "2024/25", team: "Левски София" },
    { season: "2023/24", team: "Твоят Отбор" },
    { season: "2022/23", team: "Ботев Пловдив" },
  ],
  allTimeGoalscorers: [
    { rank: 1, name: "Мартин Петров", team: "Твоят Отбор", goals: 120 },
    { rank: 2, name: "Иван Иванов", team: "Лудогорец", goals: 98 },
    { rank: 3, name: "Георги Аспарухов", team: "Левски София", goals: 95 },
    { rank: 4, name: "Емил Костадинов", team: "ЦСКА - София", goals: 89 },
    { rank: 5, name: "Стойчо Младенов", team: "Твоят Отбор", goals: 85 },
  ],
  allTimePlayerRatings: [
    { rank: 1, name: "Красимир Балъков", team: "Твоят Отбор", avgRating: 7.89 },
    { rank: 2, name: "Мартин Петров", team: "Твоят Отбор", avgRating: 7.75 },
    { rank: 3, name: "Димитър Бербатов", team: "ЦСКА - София", avgRating: 7.71 },
    { rank: 4, name: "Стилиян Петров", team: "Лудогорец", avgRating: 7.65 },
    { rank: 5, name: "Христо Стоичков", team: "Левски София", avgRating: 7.62 },
  ],
  allTimeTeamRankings: [
    { rank: 1, team: "Твоят Отбор", points: 1540 },
    { rank: 2, team: "Лудогорец", points: 1490 },
    { rank: 3, team: "Левски София", points: 1485 },
    { rank: 4, team: "ЦСКА - София", points: 1470 },
    { rank: 5, team: "Ботев Пловдив", points: 1200 },
  ]
});

const TeamLogo = ({ teamName }) => {
  const colorHash = (str) => {
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
      hash = str.charCodeAt(i) + ((hash << 5) - hash);
    }
    const colors = [
      'bg-red-500', 'bg-blue-600', 'bg-green-500', 'bg-yellow-500',
      'bg-indigo-500', 'bg-purple-500', 'bg-pink-500', 'bg-sky-600',
      'bg-emerald-500', 'bg-orange-500'
    ];
    return colors[Math.abs(hash) % colors.length];
  };

  const getInitials = (name) => name.split(' ').map(n => n[0]).slice(0, 2).join('');

  return (
    <div className={`w-8 h-8 rounded-full ${colorHash(teamName)} text-white flex 
                     items-center justify-center font-bold text-sm flex-shrink-0`}>
      {getInitials(teamName)}
    </div>
  );
};

const StatCard = ({ icon, title, value }) => (
  <div className="bg-gray-800 p-5 rounded-xl shadow-md flex items-center space-x-4 transition hover:shadow-xl hover:-translate-y-1 border border-gray-700">
    <div className="p-3 bg-gray-700 text-gray-100 rounded-full">
      {icon}
    </div>
    <div>
      <p className="text-sm font-medium text-gray-400">{title}</p>
      <p className="text-2xl font-bold text-white">{value}</p>
    </div>
  </div>
);

const RankingCard = ({ title, icon, children }) => (
  <div className="bg-gray-800 rounded-xl shadow-lg overflow-hidden flex flex-col border border-gray-700">
    <div className="flex items-center gap-3 p-4 bg-gray-900 border-b border-gray-700">
      <div className="text-gray-100">{icon}</div>
      <h3 className="text-lg font-bold text-gray-100">{title}</h3>
    </div>
    <ul className="divide-y divide-gray-700 overflow-y-auto max-h-80">
      {children}
    </ul>
  </div>
);

export default function AllTimeStats({ gameSaveId }) {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    setLoading(true);
    setError(null);
    const timer = setTimeout(() => {
      if (!gameSaveId) {
        setError("Няма зареден сейф.");
        setLoading(false);
        return;
      }
      try {
        const data = getMockData(); 
        setStats(data);
      } catch (err) {
        setError("Грешка при зареждане на мок данните.");
      } finally {
        setLoading(false);
      }
    }, 1000);
    return () => clearTimeout(timer);
  }, [gameSaveId]);

  if (loading) return <div className="flex items-center justify-center p-10 text-gray-300">Зареждане...</div>;
  if (error) return <div className="flex items-center justify-center p-10 text-red-400">{error}</div>;
  if (!stats) return <div className="flex items-center justify-center p-10 text-gray-400">Няма данни</div>;

  const { managerStats, leagueChampions, cupWinners, allTimeGoalscorers, allTimePlayerRatings, allTimeTeamRankings } = stats;

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 text-gray-100 py-10 px-4">
      <div className="container mx-auto max-w-7xl">
        <h1 className="text-3xl font-bold mb-8 text-white">Зала на Славата</h1>

        <h2 className="text-xl font-semibold mb-4 text-gray-300">Твоята Кариера</h2>
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4 mb-10">
          <StatCard icon={<BarChart size={24} />} title="Сезони" value={managerStats.totalSeasons} />
          <StatCard icon={<Trophy size={24} />} title="Трофеи" value={managerStats.trophiesWon} />
          <StatCard icon={<Percent size={24} />} title="Победи %" value={`${managerStats.winPercentage}%`} />
          <StatCard icon={<Goal size={24} />} title="Вкарани" value={managerStats.goalsScored} />
          <StatCard icon={<Shield size={24} />} title="Допуснати" value={managerStats.goalsConceded} />
        </div>

        <h2 className="text-xl font-semibold mb-4 text-gray-300">Вечни Класации</h2>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="flex flex-col gap-6">
            <RankingCard title="Вечни Голмайстори" icon={<Goal size={20} />}>
              {allTimeGoalscorers.map(player => (
                <li key={player.rank} className="flex items-center p-3 hover:bg-gray-700 transition">
                  <span className="font-bold w-10 text-gray-400">{player.rank}.</span>
                  <TeamLogo teamName={player.team} />
                  <div className="ml-3 flex-1">
                    <p className="font-semibold text-white">{player.name}</p>
                    <p className="text-sm text-gray-400">{player.team}</p>
                  </div>
                  <span className="font-bold text-lg text-gray-200">{player.goals}</span>
                </li>
              ))}
            </RankingCard>

            <RankingCard title="Най-висок Рейтинг (Кариера)" icon={<Star size={20} />}>
              {allTimePlayerRatings.map(player => (
                <li key={player.rank} className="flex items-center p-3 hover:bg-gray-700 transition">
                  <span className="font-bold w-10 text-gray-400">{player.rank}.</span>
                  <TeamLogo teamName={player.team} />
                  <div className="ml-3 flex-1">
                    <p className="font-semibold text-white">{player.name}</p>
                    <p className="text-sm text-gray-400">{player.team}</p>
                  </div>
                  <span className="font-bold text-lg text-amber-400">{player.avgRating.toFixed(2)}</span>
                </li>
              ))}
            </RankingCard>
          </div>

          <div className="flex flex-col gap-6">
            <RankingCard title="All-Time Ранкинг Отбори" icon={<Users size={20} />}>
              {allTimeTeamRankings.map(team => (
                <li key={team.rank} className="flex items-center p-3 hover:bg-gray-700 transition">
                  <span className="font-bold w-10 text-gray-400">{team.rank}.</span>
                  <TeamLogo teamName={team.team} />
                  <p className="ml-3 flex-1 font-semibold text-white">{team.team}</p>
                  <span className="font-bold text-md text-gray-200">{team.points} т.</span>
                </li>
              ))}
            </RankingCard>

            <RankingCard title="Шампиони на Лигата" icon={<Trophy size={20} />}>
              {leagueChampions.map(c => (
                <li key={c.season} className="flex items-center p-3 hover:bg-gray-700 transition">
                  <span className="font-semibold w-20 text-gray-400">{c.season}</span>
                  <TeamLogo teamName={c.team} />
                  <p className="ml-3 font-semibold text-white">{c.team}</p>
                </li>
              ))}
            </RankingCard>

            <RankingCard title="Носители на Купата" icon={<Award size={20} />}>
              {cupWinners.map(c => (
                <li key={c.season} className="flex items-center p-3 hover:bg-gray-700 transition">
                  <span className="font-semibold w-20 text-gray-400">{c.season}</span>
                  <TeamLogo teamName={c.team} />
                  <p className="ml-3 font-semibold text-white">{c.team}</p>
                </li>
              ))}
            </RankingCard>
          </div>
        </div>
      </div>
    </div>
  );
}
