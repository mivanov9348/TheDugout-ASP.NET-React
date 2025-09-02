import { NavLink, Outlet } from "react-router-dom";

function Competitions() {
  return (
    <div className="flex flex-col gap-4">
      {/* Навигация за табовете */}
      <div className="flex gap-3 border-b pb-2">
        <NavLink
          to="/competitions/league"
          className={({ isActive }) =>
            `px-4 py-2 rounded-lg font-bold transition 
            ${isActive ? "bg-sky-600 text-white" : "bg-slate-200 hover:bg-slate-300"}`
          }
        >
          League
        </NavLink>
        <NavLink
          to="/competitions/cup"
          className={({ isActive }) =>
            `px-4 py-2 rounded-lg font-bold transition 
            ${isActive ? "bg-sky-600 text-white" : "bg-slate-200 hover:bg-slate-300"}`
          }
        >
          Cup
        </NavLink>
        <NavLink
          to="/competitions/europe"
          className={({ isActive }) =>
            `px-4 py-2 rounded-lg font-bold transition 
            ${isActive ? "bg-sky-600 text-white" : "bg-slate-200 hover:bg-slate-300"}`
          }
        >
          European Cup
        </NavLink>
      </div>

      {/* Съдържанието */}
      <div className="p-4 bg-white rounded-xl shadow">
        <Outlet />
      </div>
    </div>
  );
}

export default Competitions;
