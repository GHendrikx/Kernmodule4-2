<?php
  include "DatabaseConnection.php";
  SetExistingSession();
  session_start();
  
  //Server_ID Server Password
  $server_id = $_GET["server_id"];
  $password = $_GET["password"];

  $query = "SELECT id FROM Servers WHERE server_password = '$password' AND id = '$server_id'";

  if (!($result = $mysqli->query($query)))
  showerror($mysqli->errno,$mysqli->error);

  $row = $result->fetch_assoc();

  $_SESSION["server_id"] = $row["id"];

  if($row["id"] != NULL){
    echo session_id();
  }
  else
    echo "0";
 ?>
