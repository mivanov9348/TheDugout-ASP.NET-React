function Home() {
  const sections = [
    "Next Match",
    "Last Fixtures",
    "Standings",
    "Form",
    "Top Players",
    "Transfers",
    "Finances",
    "Inbox",
    "News",
  ];

  return (
    <div className="p-6">
      <h2 className="text-2xl font-bold mb-6 text-center">
        🏟️ Dashboard
      </h2>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {sections.map((title, index) => (
          <div
            key={index}
            className="bg-white/80 backdrop-blur-sm rounded-2xl shadow-lg p-4 flex flex-col justify-between hover:scale-105 transition-transform duration-200"
          >
            <h3 className="text-lg font-semibold mb-2 text-gray-800">
              {title}
            </h3>
            <div className="flex-1 flex items-center justify-center text-gray-400 italic">
              Няма данни
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

export default Home;
