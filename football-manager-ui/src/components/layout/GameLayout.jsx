// src/components/layout/GameLayout.jsx
import React, { useState } from "react";
import Sidebar from "../Sidebar";
import Header from "../Header";
import MainContent from "../MainContent";

export default function GameLayout({ initialPage = "Home", username }) {
  const [activePage, setActivePage] = useState(initialPage);
  const handleExitGame = () => {
    // твоят exit handler - например redirect към меню
    window.location.href = "/";
  };

  return (
    <div className="flex min-h-screen">
      {/* Sidebar: фиксирана ширина, full-height */}
      <Sidebar onExitGame={handleExitGame} setActivePage={setActivePage} />

      {/* Main area: header + scrollable content */}
      <div className="flex-1 flex flex-col min-h-screen overflow-hidden">
        <Header username={username} />

        {/* съдържанието ще скролва вътре в тази зона */}
        <div className="flex-1 overflow-y-auto bg-gradient-to-br from-slate-900 via-slate-800 to-slate-700">
          {/* важно: MainContent трябва да е без собствен белез фон (bg-transparent) */}
          <MainContent activePage={activePage} />
        </div>
      </div>
    </div>
  );
}
