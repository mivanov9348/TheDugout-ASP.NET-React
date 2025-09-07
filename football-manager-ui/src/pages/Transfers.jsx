import { NavLink, Outlet, useNavigate } from "react-router-dom";


const Transfers = () => {

    const navigate = useNavigate();

  return (
    <div className="space-y-6">

      <div className="bg-white shadow rounded-2xl p-4 border border-slate-200">
        <h1 className="text-3xl font-bold text-slate-800">Transfers</h1>
        <p className="text-slate-500 text-sm mt-1">
          Manage player transfers, ongoing negotiations and your club’s transfer history.
        </p>
      </div>

      <div className="bg-white shadow rounded-2xl border border-slate-200">
        <nav className="flex gap-3 px-4 pt-3 border-b">
          {[
            { path: "search", label: "Search Players" },
            { path: "negotiations", label: "Negotiations" },
            { path: "history", label: "Transfer History" },
          ].map((tab) => (
            <NavLink
              key={tab.path}
              to={tab.path}
              end
              className={({ isActive }) =>
                `px-4 py-2 text-sm font-medium rounded-t-lg transition-colors
                ${
                  isActive
                    ? "bg-sky-600 text-white shadow"
                    : "text-slate-600 hover:bg-slate-100"
                }`
              }
            >
              {tab.label}
            </NavLink>
          ))}
        </nav>

        {/* Подстраници */}
        <div className="p-6">
          <Outlet />
        </div>
      </div>
    </div>
  );
};

export default Transfers;
