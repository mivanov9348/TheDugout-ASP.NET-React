import { useState, useEffect } from "react";
import { 
  BarChart, Trophy, Goal, Shield, Percent, TrendingUp, 
  TrendingDown, Star, Award, User, Users, List 
} from "lucide-react";

// --- Мок Данни ---
// Създаваме богат обект с мок данни, който да симулира отговора от API
const getMockData = () => ({
  // 1. Общи статистики на мениджъра (за горните карти)
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
  // 2. Списък с шампиони на лигата
  leagueChampions: [
    { season: "2024/25", team: "Твоят Отбор" },
    { season: "2023/24", team: "Лудогорец" },
    { season: "2022/23", team: "ЦСКА - София" },
  ],
  // 3. Списък с носители на купата
  cupWinners: [
    { season: "2024/25", team: "Левски София" },
    { season: "2023/24", team: "Твоят Отбор" },
    { season: "2022/23", team: "Ботев Пловдив" },
  ],
  // 4. Вечни голмайстори (Добавени още, за да тестваме скрола)
  allTimeGoalscorers: [
    { rank: 1, name: "Мартин Петров", team: "Твоят Отбор", goals: 120 },
    { rank: 2, name: "Иван Иванов", team: "Лудогорец", goals: 98 },
    { rank: 3, name: "Георги Аспарухов", team: "Левски София", goals: 95 },
    { rank: 4, name: "Емил Костадинов", team: "ЦСКА - София", goals: 89 },
    { rank: 5, name: "Стойчо Младенов", team: "Твоят Отбор", goals: 85 },
    { rank: 6, name: "Петър Жеков", team: "ЦСКА - София", goals: 81 },
    { rank: 7, name: "Наско Сираков", team: "Левски София", goals: 79 },
    { rank: 8, name: "Пламен Гетов", team: "Ботев Пловдив", goals: 77 },
    { rank: 9, name: "Божидар Искренов", team: "Левски София", goals: 76 },
    { rank: 10, name: "Георги Димитров", team: "ЦСКА - София", goals: 75 },
  ],
  // 5. Играчи с най-висок рейтинг
  allTimePlayerRatings: [
    { rank: 1, name: "Красимир Балъков", team: "Твоят Отбор", avgRating: 7.89 },
    { rank: 2, name: "Мартин Петров", team: "Твоят Отбор", avgRating: 7.75 },
    { rank: 3, name: "Димитър Бербатов", team: "ЦСКА - София", avgRating: 7.71 },
    { rank: 4, name: "Стилиян Петров", team: "Лудогорец", avgRating: 7.65 },
    { rank: 5, name: "Христо Стоичков", team: "Левски София", avgRating: 7.62 },
  ],
  // 6. All-Time ранкинг на отборите (по точки/коефициент)
  allTimeTeamRankings: [
    { rank: 1, team: "Твоят Отбор", points: 1540 },
    { rank: 2, team: "Лудогорец", points: 1490 },
    { rank: 3, team: "Левски София", points: 1485 },
    { rank: 4, team: "ЦСКА - София", points: 1470 },
    { rank: 5, team: "Ботев Пловдив", points: 1200 },
  ]
});

// --- Помощни Компоненти ---

/**
 * Малка цветна кръгла икона с инициалите на отбора
 */
const TeamLogo = ({ teamName }) => {
  // Проста хеш функция за генериране на цвят от името
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

  // Взимаме инициали (напр. "Левски София" -> "ЛС")
  const getInitials = (name) => {
    return name.split(' ').map(n => n[0]).slice(0, 2).join('');
  };

  return (
    <div className={`w-8 h-8 rounded-full ${colorHash(teamName)} text-white flex 
                     items-center justify-center font-bold text-sm flex-shrink-0`}>
      {getInitials(teamName)}
    </div>
  );
};

/**
 * Горните карти със статистика
 */
const StatCard = ({ icon, title, value }) => (
  <div className="bg-white p-5 rounded-xl shadow-md flex items-center space-x-4 transition hover:shadow-lg hover:-translate-y-1">
    <div className="p-3 bg-sky-100 text-sky-600 rounded-full">
      {icon}
    </div>
    <div>
      <p className="text-sm font-medium text-gray-500">{title}</p>
      <p className="text-2xl font-bold text-gray-900">{value}</p>
    </div>
  </div>
);

/**
 * Карта-контейнер за всяка класация
 */
const RankingCard = ({ title, icon, children }) => (
  <div className="bg-white rounded-xl shadow-lg overflow-hidden flex flex-col">
    {/* Хедър на картата */}
    <div className="flex items-center gap-3 p-4 bg-slate-50 border-b border-slate-200">
      <div className="text-sky-600">{icon}</div>
      <h3 className="text-lg font-bold text-slate-800">{title}</h3>
    </div>
    {/* Съдържание (листа) - с overflow и max-height */}
    <ul className="divide-y divide-slate-100 overflow-y-auto max-h-80">
      {children}
    </ul>
  </div>
);


// --- Основен Компонент ---
export default function AllTimeStats({ gameSaveId }) {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    setLoading(true);
    setError(null);
    
    // Симулация на API зареждане
    const timer = setTimeout(() => {
      if (!gameSaveId) {
        setError("Няма зареден сейф.");
        setLoading(false);
        return;
      }
      try {
        // Вместо fetch, просто викаме мок данните
        const data = getMockData(); 
        setStats(data);
      } catch (err) {
        console.error("Error loading mock data:", err);
        setError("Грешка при зареждане на мок данните.");
      } finally {
        setLoading(false);
      }
    }, 1000); // 1 секунда забавяне

    // Почистваме таймера
    return () => clearTimeout(timer);
  }, [gameSaveId]); // Все още зависим от gameSaveId, за да "зареди"

  if (loading) {
    return <div className="flex items-center justify-center p-10">
      <p className="text-lg text-gray-600 animate-pulse">Зареждане на статистиката на кариерата...</p>
    </div>;
  }

  if (error) {
    return <div className="flex items-center justify-center p-10">
       <p className="text-lg text-red-500">Грешка: {error}</p>
    </div>;
  }

  if (!stats) {
    return <div className="flex items-center justify-center p-10">
      <p className="text-lg text-gray-500">Няма налична статистика за показване.</p>
    </div>;
  }

  // Деструктурираме мок данните за по-лесно ползване
  const { 
    managerStats, 
    leagueChampions, 
    cupWinners, 
    allTimeGoalscorers, 
    allTimePlayerRatings,
    allTimeTeamRankings
  } = stats;

  return (
    <div className="container mx-auto p-4 max-w-7xl">
      <h1 className="text-3xl font-bold mb-6 text-gray-800">Зала на Славата</h1>
      
      {/* Секция 1: Статистика на Мениджъра (Твоята) */}
      <h2 className="text-xl font-semibold mb-4 text-slate-700">Твоята Кариера</h2>
      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4 mb-8">
        <StatCard 
          icon={<BarChart size={24} />} 
          title="Сезони" 
          value={managerStats.totalSeasons} 
        />
        <StatCard 
          icon={<Trophy size={24} />} 
          title="Трофеи" 
          value={managerStats.trophiesWon} 
        />
        <StatCard 
          icon={<Percent size={24} />} 
          title="Победи %" 
          value={`${managerStats.winPercentage}%`}
        />
        <StatCard 
          icon={<Goal size={24} />} 
          title="Вкарани" 
          value={managerStats.goalsScored}
        />
         <StatCard 
          icon={<Shield size={24} />} 
          title="Допуснати" 
          value={managerStats.goalsConceded}
        />
      </div>

      {/* Секция 2: Класации */}
      <h2 className="text-xl font-semibold mb-4 text-slate-700">Вечни Класации</h2>
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">

        {/* --- ЛЯВА КОЛОНА --- */}
        <div className="flex flex-col gap-6">
          {/* Вечни голмайстори */}
          <RankingCard title="Вечни Голмайстори" icon={<Goal size={20} />}>
            {allTimeGoalscorers.map(player => (
              <li key={player.rank} className="flex items-center p-3 hover:bg-slate-50 transition">
                <span className="font-bold w-10 text-slate-500">{player.rank}.</span>
                <TeamLogo teamName={player.team} />
                <div className="ml-3 flex-1">
                  <p className="font-semibold text-slate-900">{player.name}</p>
                  <p className="text-sm text-slate-600">{player.team}</p>
                </div>
                <span className="font-bold text-lg text-sky-600">{player.goals}</span>
              </li>
            ))}
          </RankingCard>

          {/* Играчи с най-висок рейтинг */}
          <RankingCard title="Най-висок Рейтинг (Кариера)" icon={<Star size={20} />}>
            {allTimePlayerRatings.map(player => (
              <li key={player.rank} className="flex items-center p-3 hover:bg-slate-50 transition">
                <span className="font-bold w-10 text-slate-500">{player.rank}.</span>
                <TeamLogo teamName={player.team} />
                <div className="ml-3 flex-1">
                  <p className="font-semibold text-slate-900">{player.name}</p>
                  <p className="text-sm text-slate-600">{player.team}</p>
                </div>
                <span className="font-bold text-lg text-amber-500">{player.avgRating.toFixed(2)}</span>
              </li>
            ))}
          </RankingCard>
        </div>

        {/* --- ДЯСНА КОЛОНА --- */}
        <div className="flex flex-col gap-6">
          {/* Ранкинг на отборите */}
           <RankingCard title="All-Time Ранкинг Отбори" icon={<Users size={20} />}>
            {allTimeTeamRankings.map(team => (
              <li key={team.rank} className="flex items-center p-3 hover:bg-slate-50 transition">
                <span className="font-bold w-10 text-slate-500">{team.rank}.</span>
                <TeamLogo teamName={team.team} />
                <p className="ml-3 flex-1 font-semibold text-slate-900">{team.team}</p>

                <span className="font-bold text-md text-slate-700">{team.points} т.</span>
              </li>
            ))}
          </RankingCard>

          {/* Шампиони на Лигата */}
          <RankingCard title="Шампиони на Лигата" icon={<Trophy size={20} />}>
            {leagueChampions.map(c => (
              <li key={c.season} className="flex items-center p-3 hover:bg-slate-50 transition">
                <span className="font-semibold w-20 text-slate-500">{c.season}</span>
                <TeamLogo teamName={c.team} />
                <p className="ml-3 font-semibold text-slate-900">{c.team}</p>
              </li>
            ))}
          </RankingCard>

           {/* Носители на Купата */}
           <RankingCard title="Носители на Купата" icon={<Award size={20} />}>
            {cupWinners.map(c => (
              <li key={c.season} className="flex items-center p-3 hover:bg-slate-50 transition">
                <span className="font-semibold w-20 text-slate-500">{c.season}</span>
                <TeamLogo teamName={c.team} />
                <p className="ml-3 font-semibold text-slate-900">{c.team}</p>
              </li>
            ))}
          </RankingCard>
        </div>
      </div>
    </div>
  );
}

