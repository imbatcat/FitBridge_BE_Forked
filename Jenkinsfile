pipeline {
    agent {
        node {
            label 'docker-agent-dotnet'
        }
    }
    environment {
        REGISTRY = 'rutkre'
        IMAGE_NAME = 'fitbridge-be'
        DOCKER_BUILDKIT = '1' // use docker buildx
    }
    stages {
        stage('Build') {
            steps {
                echo "Building.."
                sh '''
                    dotnet build --property:WarningLevel=0 --configuration Release
                '''
            }
        }
        stage('Test') {
            steps {
                echo "Testing.."
                sh '''
                    dotnet test --no-build --configuration Release
                '''
            }
        }
        stage('Build & Push') {
            steps {
                script {
                    withCredentials([usernamePassword(
                        credentialsId: 'docker-credentials',
                        usernameVariable: 'DOCKER_USER',
                        passwordVariable: 'DOCKER_PASS'
                    )]) {
                        sh '''
                            # Authenticate through docker hub
                            mkdir -p ~/.docker
                            
                            cat > ~/.docker/config.json << EOF
                            {
                                "auths": {
                                    "https://index.docker.io/v1/": {
                                        "auth": "$(echo -n "$DOCKER_USER:$DOCKER_PASS" | base64)"
                                    }
                                }
                            }
                            EOF
                            
                            IMAGE_TAG=$(git rev-parse HEAD | sha256sum | cut -d' ' -f1)
                            
                            docker buildx build \
                                --push \
                                -t rutkre/fitbridge-be:${IMAGE_TAG} \
                                -t rutkre/fitbridge-be:latest \
                                --build-arg BUILDKIT_INLINE_CACHE=1 \
                                --cache-from rutkre/fitbridge-be:latest \
                                .
                            
                            # Cleanup
                            rm -f ~/.docker/config.json
                        '''
                    }
                }
            }
        }
    }
}
