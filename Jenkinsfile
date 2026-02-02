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
                    dotnet build --property:WarningLevel=1 --configuration Release
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
                    sh 'docker buildx version'
                    def imagePath = "rutkre/fitbridge-be"
                    def credsId = 'docker-credentials'

                    sh '''
                            
                            # Build with buildx using inline cache
                            docker buildx build \
                            --push \
                            -t rutkre/fitbridge-be:36 \
                            -t rutkre/fitbridge-be:latest \
                            --build-arg BUILDKIT_INLINE_CACHE=1 \
                            --cache-from rutkre/fitbridge-be:latest \
                    .
                        
                    docker logout
                    '''
                }
            }
        }
    }
}
