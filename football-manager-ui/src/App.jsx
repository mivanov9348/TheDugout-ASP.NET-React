import { useState } from "react";
import Header from "./components/Header";
import Sidebar from "./components/Sidebar";
import MainContent from "./components/MainContent";

function App() {
  const [activePage, setActivePage] = useState("Home");

  return (
    <div className="grid grid-rows-[60px_1fr] grid-cols-[220px_1fr] h-screen">
      <Header title="The Dugout" />
      <Sidebar activePage={activePage} setActivePage={setActivePage} />
      <MainContent activePage={activePage} />
    </div>
  );
}

export default App;
