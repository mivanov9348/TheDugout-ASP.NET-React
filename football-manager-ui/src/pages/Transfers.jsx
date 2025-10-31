import { NavLink, Outlet, useNavigate } from "react-router-dom";

const Transfers = () => {
  const navigate = useNavigate();

  return (
    <div className="space-y-6 h-[calc(100vh-100px)] flex flex-col bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900 text-gray-100">
      {/* Header */}
      <div className="bg-gray-800 rounded-2xl p-5 border border-gray-700 shadow-lg">
        <h1 className="text-3xl font-bold text-gray-100">Transfers</h1>
      </div>

      {/* Content with tabs */}
      <div className="bg-gray-800 rounded-2xl border border-gray-700 shadow-lg flex flex-col flex-1 overflow-hidden">
        <nav className="flex gap-3 px-4 pt-3 border-b border-gray-700">
          {[
            { path: "/transfers/search", label: "Search Players" },
            { path: "/transfers/negotiations", label: "Negotiations" },
            { path: "/transfers/history", label: "Transfer History" },
          ].map((tab) => (
            <NavLink
              key={tab.path}
              to={tab.path}
              end
              className={({ isActive }) =>
                `px-4 py-2 text-sm font-medium rounded-t-lg transition-all duration-200 ${isActive
                  ? "bg-blue-600 text-white shadow-md"
                  : "text-gray-400 hover:bg-gray-700 hover:text-white"
                }`
              }
            >
              {tab.label}
            </NavLink>
          ))}

        </nav>

        {/* Outlet content */}
        <div className="flex-1 overflow-auto p-4 bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900">
          <Outlet />
        </div>
      </div>
    </div>
  );
};

export default Transfers;
