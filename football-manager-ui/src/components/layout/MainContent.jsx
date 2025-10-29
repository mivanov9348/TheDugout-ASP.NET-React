// src/components/MainContent.jsx
import Club from "../../pages/club/Club";
import Home from "../../pages/Home";
import Inbox from "../../pages/Inbox";
import Calendar from "../../pages/season/Calendar";
import Squad from "../../pages/club/Squad";
import Tactics from "../../pages/club/Tactics";
import Training from "../../pages/club/Training";
import Fixtures from "../../pages/season/Fixtures";
import Transfers from "../../pages/Transfers";
import Finances from "../../pages/club/Finances";

function MainContent({ activePage }) {
  return (
    <main className="flex-1 bg-transparent p-0 overflow-y-auto">
      {/* Всяка страница да има собствен container/padding */}
      {activePage === "Home" && <Home />}
      {activePage === "Inbox" && <Inbox />}
      {activePage === "Calendar" && <Calendar />}
      {activePage === "Squad" && <Squad />}
      {activePage === "Tactics" && <Tactics />}
      {activePage === "Training" && <Training />}
      {activePage === "Fixtures" && <Fixtures />}
      {activePage === "Competitions" && (
        <div className="p-6">
          <h1 className="text-2xl font-bold mb-4">Competitions</h1>
          <p>Manage your competitions here.</p>
        </div>
      )}
      {activePage === "Transfers" && <Transfers />}
      {activePage === "Club" && <Club />}
      {activePage === "Finances" && <Finances />}
      {activePage === "Players" && (
        <div className="p-6">
          <h1 className="text-2xl font-bold mb-4">Players</h1>
          <p>Manage your players here.</p>
        </div>
      )}
    </main>
  );
}

export default MainContent;
