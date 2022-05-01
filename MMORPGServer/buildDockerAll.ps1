cp ../MMORPGClient/Assets/StreamingAssets/Maps/town.map ./
cp ../MMORPGClient/Assets/StreamingAssets/Quests/* ./Quests/
cp ../MMORPGClient/Assets/StreamingAssets/AoESkills/* ./AoESkills/
$services = "LoginService","WorldChatService","CharacterService","MapService","DailyLoginService","InventoryEventProcessorService","PlayerEventProcessorService","PlayerEventService","QuestService","CombatService","InventoryService","EnemyLoadBalancerService","EnemyWorkerService"

foreach ($service in $services)
{
  Write-Host Build $service
  docker build -t "$($service.ToLower()):latest" -f ./$service/Dockerfile ./

  if($LASTEXITCODE -ne 0) {
    Write-Host "Error building image $service."
    exit;
  }
}
