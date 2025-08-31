import Club from "../pages/Club";
import Finances from "../pages/Finances";
import Home from "../pages/Home";
import Inbox from "../pages/Inbox";
import League from "../pages/League";
import Schedule from "../pages/Schedule";
import Squad from "../pages/Squad";
import Tactics from "../pages/Tactics";
import Training from "../pages/Training";
import Transfers from "../pages/Transfers";
import Calendar from "../pages/Calendar";

function MainContent({ activePage }) {
  return (
    <main className="flex-1 bg-white p-6 overflow-y-auto">
      {activePage === "Home" && <Home />}
      {activePage === "Inbox" && <Inbox />}
      {activePage === "Calendar" && <Calendar />}
      {activePage === "Squad" && <Squad />}
      {activePage === "Tactics" && <Tactics />}
      {activePage === "Training" && <Training />}
      {activePage === "Schedule" && <Schedule />}
      {activePage === "League" && <League />}
      {activePage === "Transfers" && <Transfers />}
      {activePage === "Club" && <Club />}
      {activePage === "Finances" && <Finances />}
      {activePage === "Players" && <div><h1 className="text-2xl font-bold mb-4">Players</h1><p>Manage your players here.</p></div>}
    </main>
  );
}

export default MainContent;
