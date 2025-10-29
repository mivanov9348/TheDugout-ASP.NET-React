import { NavLink, Outlet } from "react-router-dom";

function Competitions() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 text-gray-200 p-6  shadow-2xl">
      {/* Навигация за табовете */}
      <div className="flex gap-3 border-b border-gray-700 pb-2 mb-4">
        <NavLink
          to="/competitions/league"
          className={({ isActive }) =>
            `px-4 py-2 rounded-lg font-semibold transition-all duration-200 
            ${
              isActive
                ? "bg-sky-600 text-white shadow-md shadow-sky-700/40"
                : "bg-gray-800 hover:bg-gray-700 text-gray-300 border border-gray-700"
            }`
          }
        >
          League
        </NavLink>

        <NavLink
          to="/competitions/cup"
          className={({ isActive }) =>
            `px-4 py-2 rounded-lg font-semibold transition-all duration-200 
            ${
              isActive
                ? "bg-sky-600 text-white shadow-md shadow-sky-700/40"
                : "bg-gray-800 hover:bg-gray-700 text-gray-300 border border-gray-700"
            }`
          }
        >
          Cup
        </NavLink>

        <NavLink
          to="/competitions/europe"
          className={({ isActive }) =>
            `px-4 py-2 rounded-lg font-semibold transition-all duration-200 
            ${
              isActive
                ? "bg-sky-600 text-white shadow-md shadow-sky-700/40"
                : "bg-gray-800 hover:bg-gray-700 text-gray-300 border border-gray-700"
            }`
          }
        >
          European Cup
        </NavLink>
      </div>

      {/* Съдържанието */}
      <div className="p-5 bg-gray-800/60 backdrop-blur-sm border border-gray-700 rounded-xl shadow-lg transition-all">
        <Outlet />
      </div>
    </div>
  );
}

export default Competitions;
