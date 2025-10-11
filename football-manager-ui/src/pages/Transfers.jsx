import { NavLink, Outlet, useNavigate } from "react-router-dom";

const Transfers = () => {
  const navigate = useNavigate();

  return (
    <div className="space-y-6 h-[calc(100vh-100px)] flex flex-col">
      {/* Header */}
      <div className="bg-white shadow rounded-2xl p-4 border border-slate-200">
        <h1 className="text-3xl font-bold text-slate-800">Transfers</h1>
       
      </div>

      {/* Content with tabs */}
      <div className="bg-white shadow rounded-2xl border border-slate-200 flex flex-col flex-1 overflow-hidden">
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

        {/* Outlet content */}
        <div className="flex-1 overflow-auto p-4">
          <Outlet />
        </div>
      </div>
    </div>
  );
};

export default Transfers;
