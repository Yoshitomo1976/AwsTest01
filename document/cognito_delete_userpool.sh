 
userpool_id=$(aws cognito-idp list-user-pools --max-results 10 --query 'UserPools[0].Id' --output text)
echo $userpool_id

# ドメイン削除
domain_prefix=$(aws cognito-idp describe-user-pool --user-pool-id $userpool_id --query 'UserPool.Domain' --output text)
echo $domain_prefix
aws cognito-idp delete-user-pool-domain --user-pool-id $userpool_id --domain $domain_prefix

# クライアント削除
client_id=$(aws cognito-idp list-user-pool-clients --user-pool-id $userpool_id --query 'UserPoolClients[0].ClientId' --output text)
aws cognito-idp delete-user-pool-client --user-pool-id $userpool_id --client-id $client_id


# ユーザープール削除：実行注意
aws cognito-idp delete-user-pool --user-pool-id $userpool_id
