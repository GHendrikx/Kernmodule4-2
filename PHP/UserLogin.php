<?php
  include 'DatabaseConnection.php';

	SetExistingSession();
	session_start();

	if (!CheckServerID()) {
		echo "0";
		return;
	}
	
  //User_ID Game_ID Score
  $user_id = $_GET["userid"];
  $password = $_GET["password"];
  $_SESSION["UserID"] = $user_id;
  $_SESSION["Password"] = $password;

  $query = "SELECT * FROM Users WHERE Password = '$password' AND Username = '$user_id'";

  if (!($result = $mysqli->query($query)))
  showerror($mysqli->errno,$mysqli->error);

  $row = $result->fetch_assoc();

    if($row["Username"] == $user_id && $row["Password"] == $password)
      echo json_encode($row);
    else
      echo "0";


?>
