New-SelfSignedCertificate -DnsName localhost -CertStoreLocation cert:\LocalMachine\My
netsh http add sslcert ipport=0.0.0.0:port certhash= appid={5DC6642C-6C5B-4445-8ED7-47E5EE77FCD9}