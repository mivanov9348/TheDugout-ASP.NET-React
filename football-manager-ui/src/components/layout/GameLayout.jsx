// src/components/GameLayout.jsx
import React from "react";
import { useLocation } from "react-router-dom";
import Sidebar from "./Sidebar";
import Header from "./Header";

export default function GameLayout({ username, onExitGame, children }) {
  const location = useLocation();

  return (
    <div className="flex min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-700">
      {/* Sidebar и Header си остават тъмни с бял текст */}
      <Sidebar onExitGame={onExitGame} />

      <div className="flex-1 flex flex-col min-h-screen overflow-hidden">
        <Header key={location.pathname} username={username} />

        {/* Основно съдържание – фон по избор, не наследява text-white */}
        <main className="flex-1 overflow-y-auto p-0.3 bg-slate-100 text-slate-800">
          {children}
        </main>
      </div>
    </div>
  );
}
