import { useState } from "react";

const TeamLogo = ({ teamName, logoFileName, className = "" }) => {
  const [imgError, setImgError] = useState(false);

  const logoPath = `/logos/${logoFileName || teamName}.png`;

  const handleError = () => {
    setImgError(true);
  };

  if (imgError) {
    return (
      <div className={`w-8 h-8 bg-gray-300 rounded flex items-center justify-center text-xs font-bold ${className}`}>
        {teamName.charAt(0).toUpperCase()}
      </div>
    );
  }

  return (
    <img
      src={logoPath}
      alt={`${teamName} logo`}
      className={`w-8 h-8 object-contain ${className}`}
      onError={handleError}
      loading="lazy"
    />
  );
};

export default TeamLogo;