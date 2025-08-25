import { 
  Home, Mail, Users, Activity, Dumbbell, Calendar, 
  Trophy, ShoppingCart, Building, Wallet 
} from "lucide-react";

function Sidebar({ activePage, setActivePage }) {
  const menus = [
    { name: "Home", icon: <Home size={18} /> },
    { name: "Inbox", icon: <Mail size={18} /> },
    { name: "Squad", icon: <Users size={18} /> },
    { name: "Tactics", icon: <Activity size={18} /> },
    { name: "Training", icon: <Dumbbell size={18} /> },
    { name: "Schedule", icon: <Calendar size={18} /> },
    { name: "League", icon: <Trophy size={18} /> },
    { name: "Transfers", icon: <ShoppingCart size={18} /> },
    { name: "Club", icon: <Building size={18} /> },
    { name: "Finances", icon: <Wallet size={18} /> },
  ];

  return (
    <aside className="bg-slate-800 text-white w-56 h-screen p-4 flex flex-col">
      <h2 className="text-xl font-bold mb-6 text-center text-sky-400">The Dugout</h2>
      <ul className="space-y-2">
        {menus.map((menu) => (
          <li
            key={menu.name}
            className={`flex items-center gap-3 cursor-pointer px-4 py-2 rounded-md transition 
              ${menu.name === activePage ? "bg-sky-600 font-bold" : "hover:bg-slate-700"}`}
            onClick={() => setActivePage(menu.name)}
          >
            {menu.icon}
            <span>{menu.name}</span>
          </li>
        ))}
      </ul>
    </aside>
  );
}

export default Sidebar;
