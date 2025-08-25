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

function MainContent({ activePage }) {
  return (
    <main className="bg-white p-6 overflow-y-auto rounded-tl-xl shadow-inner">
      {activePage === "Home" && <Home />}
      {activePage === "Inbox" && <Inbox />}
      {activePage === "Squad" && <Squad />}
      {activePage === "Tactics" && <Tactics />}
      {activePage === "Training" && <Training />}
      {activePage === "Schedule" && <Schedule />}
      {activePage === "League" && <League />}
      {activePage === "Transfers" && <Transfers />}
      {activePage === "Club" && <Club />}
      {activePage === "Finances" && <Finances />}
    </main>
  );
}

export default MainContent;
