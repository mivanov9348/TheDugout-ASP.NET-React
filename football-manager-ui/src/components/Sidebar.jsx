import { NavLink, useNavigate } from "react-router-dom";
import { 
  Home, Mail, Users, Activity, Dumbbell, Calendar, 
  Trophy, ShoppingCart, Building, Wallet, Calendar1, LogOut
} from "lucide-react";

function Sidebar({ onExitGame }) {
  const menus = [
    { name: "Home", path: "/", icon: <Home size={18} /> },
    { name: "Inbox", path: "/inbox", icon: <Mail size={18} /> },
    { name: "Calendar", path: "/calendar", icon: <Calendar1 size={18} /> },
    { name: "Squad", path: "/squad", icon: <Users size={18} /> },
    { name: "Tactics", path: "/tactics", icon: <Activity size={18} /> },
    { name: "Training", path: "/training", icon: <Dumbbell size={18} /> },
    { name: "Schedule", path: "/schedule", icon: <Calendar size={18} /> },
    { name: "League", path: "/league", icon: <Trophy size={18} /> },
    { name: "Transfers", path: "/transfers", icon: <ShoppingCart size={18} /> },
    { name: "Club", path: "/club", icon: <Building size={18} /> },
    { name: "Finances", path: "/finances", icon: <Wallet size={18} /> },
    { name: "Players", path: "/players", icon: <Users size={18} /> },
  ];

  return (
    <aside className="bg-slate-800 text-white w-56 h-screen p-4 flex flex-col">
      <h2 className="text-xl font-bold mb-6 text-center text-sky-400">The Dugout</h2>
      <ul className="space-y-2 flex-1">
        {menus.map((menu) => (
          <li key={menu.name}>
            <NavLink
              to={menu.path}
              end
              className={({ isActive }) =>
                `flex items-center gap-3 px-4 py-2 rounded-md transition 
                ${isActive ? "bg-sky-600 font-bold" : "hover:bg-slate-700"}`
              }
            >
              {menu.icon}
              <span>{menu.name}</span>
            </NavLink>
          </li>
        ))}
      </ul>

      {/* Exit Game бутон */}
      <button
        onClick={onExitGame}
        className="flex items-center gap-3 px-4 py-2 rounded-md hover:bg-red-600 transition bg-red-500 text-white font-bold"
      >
        <LogOut size={18} />
        Exit Game
      </button>
    </aside>
  );
}

export default Sidebar;
