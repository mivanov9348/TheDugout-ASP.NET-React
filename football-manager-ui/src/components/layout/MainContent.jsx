import Club from "../pages/Club";
import Finances from "../pages/Finances";
import Home from "../../pages/Home";
import Inbox from "../../pages/Inbox";
import Squad from "../pages/Squad";
import Tactics from "../pages/Tactics";
import Training from "../pages/Training";
import Transfers from "../../pages/Transfers";
import Calendar from "../pages/Calendar";
import Fixtures from "../pages/Fixtures";

function MainContent({ activePage }) {
  return (
   <main className="flex-1 bg-transparent p-0 overflow-y-auto">
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
