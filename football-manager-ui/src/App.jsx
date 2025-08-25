import './App.css'
import { useEffect, useState } from "react";

function App() {
  const [msg, setMsg] = useState("");

  useEffect(() => {
    fetch("https://localhost:7117/api/hello")
      .then(r => r.text())
      .then(setMsg);
  }, []);

  return <h1>{msg}</h1>;
}

export default App;
