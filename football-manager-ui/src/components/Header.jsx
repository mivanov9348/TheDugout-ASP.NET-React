function Header() {
  const teamName = "FC Barcelona";
  const leagueName = "La Liga";

  return (
    <header className="col-span-2 flex justify-between items-center px-6 py-3 bg-slate-800 text-white shadow-md">
      {/* Отбор + Лига */}
      <div>
        <h1 className="text-lg font-bold">{teamName}</h1>
        <p className="text-sm text-slate-300">{leagueName}</p>
      </div>

      {/* Next Day бутон */}
      <button className="bg-sky-600 hover:bg-sky-700 text-white px-4 py-2 rounded-lg font-medium transition">
        Next Day →
      </button>
    </header>
  );
}

export default Header;
