import { useState } from "react";
import Header from "./components/Header";
import Sidebar from "./components/Sidebar";
import MainContent from "./components/MainContent";
import StartScreen from "./components/StartScreen";

function App() {
  const [gameStarted, setGameStarted] = useState(false);
  const [activePage, setActivePage] = useState("Home");

  if (!gameStarted) {
    return <StartScreen onNewGame={() => setGameStarted(true)} />;
  }

  return (
    <div className="flex h-screen bg-slate-100">
      {/* Sidebar left */}
      <Sidebar activePage={activePage} setActivePage={setActivePage} />

      {/* Right side (Header + MainContent) */}
      <div className="flex flex-col flex-1">
        <Header />
        <MainContent activePage={activePage} />
      </div>
    </div>
  );
}

export default App;
