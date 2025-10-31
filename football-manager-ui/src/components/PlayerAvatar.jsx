import { useEffect, useState } from "react";

const PlayerAvatar = ({ playerName, imageFileName, className = "" }) => {
  const [imgError, setImgError] = useState(false);

  // Изчисляваме името на файла
  const fileName = imageFileName || `${playerName}.jpg`;
  const imagePath = `https://localhost:7117/avatars/${fileName}`;

  // 👉 Добавяме useEffect, за да логваме при всяка промяна
  useEffect(() => {
    console.groupCollapsed("🧩 PlayerAvatar Debug");
    console.log("➡️ playerName:", playerName);
    console.log("➡️ imageFileName:", imageFileName);
    console.log("➡️ fileName (final):", fileName);
    console.log("➡️ imagePath (src):", imagePath);
    console.log("➡️ imgError:", imgError);
    console.groupEnd();
  }, [playerName, imageFileName, fileName, imagePath, imgError]);

  // 👉 Проверки за липсващи данни
  if (!playerName) {
    console.warn("⚠️ PlayerAvatar: 'playerName' is missing!");
  }
  if (!imageFileName) {
    console.info("ℹ️ PlayerAvatar: No custom image, using default:", fileName);
  }

  // 👉 Ако има грешка при зареждане на снимката
  if (imgError || !fileName) {
    console.error(`🚫 Could not load avatar for: ${playerName}`);
    return (
      <div
        className={`w-10 h-10 bg-gray-300 rounded-full flex items-center justify-center text-sm font-bold ${className}`}
      >
        {playerName
          ?.split(" ")
          .map((n) => n.charAt(0).toUpperCase())
          .join("")}
      </div>
    );
  }

  // 👉 Основно изображение
  return (
    <img
      src={imagePath}
      alt={`${playerName} avatar`}
      className={`w-10 h-10 rounded-full object-cover ${className}`}
      onError={() => {
        console.error(`❌ Image failed to load: ${imagePath}`);
        setImgError(true);
      }}
      loading="lazy"
    />
  );
};

export default PlayerAvatar;
