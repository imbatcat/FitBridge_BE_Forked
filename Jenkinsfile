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
                        try {
                            // Authenticate through docker hub w/ buildkit
                            echo $DOCKER_PASS | docker login -u $DOCKER_USER --password-stdin

                            // Build and push
                            sh '''
                                IMAGE_TAG=$(git rev-parse HEAD | sha256sum | cut -d' ' -f1)
                                docker buildx build \
                                    --push \
                                    -t rutkre/fitbridge-be:${IMAGE_TAG} \
                                    -t rutkre/fitbridge-be:latest \
                                    --build-arg BUILDKIT_INLINE_CACHE=1 \
                                    --cache-from rutkre/fitbridge-be:latest \
                                    .
                            '''
                        } finally {
                            // Cleanup
                                // docker logout https://index.docker.io/v1/ || true
                            sh '''
                                docker logout
                                if (fileExists('~/.docker/config.json')) {
                                    sh 'rm -f ~/.docker/config.json'
                                }
                            '''
                        }
                    }
                }
            }
        }
    }
}
